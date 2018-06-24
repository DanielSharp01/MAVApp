using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using MAVAppBackend.DataAccess;

namespace MAVAppBackend
{
    public class DatabaseLegacy
    {
        /// <summary>
        /// MySQL connection used throughout the application
        /// </summary>
        public static MySqlConnection connection; //TODO: change this back

        /// <summary>
        /// Initializes MySQL data connection
        /// </summary>
        public static void Initialize()
        {
            connection = new MySqlConnection("Host=127.0.0.1;Database=mavapp;UserName=root;Password=mysql");
            connection.Open();
        }

        /// <summary>
        /// Normalizes a station name for comparison (removes Hungarian accents, replaces hyphens with spaces, removes redundant information such as station)
        /// </summary>
        /// <param name="stationName">Name to normalize</param>
        /// <returns>Normalized version of the same name</returns>
        public static string StationNormalizeName(string stationName)
        {
            stationName = stationName.ToLower();

            stationName = stationName.Replace('á', 'a');
            stationName = stationName.Replace('é', 'e');
            stationName = stationName.Replace('í', 'i');
            stationName = stationName.Replace('ó', 'o');
            stationName = stationName.Replace('ö', 'o');
            stationName = stationName.Replace('ő', 'o');
            stationName = stationName.Replace('ú', 'u');
            stationName = stationName.Replace('ü', 'u');
            stationName = stationName.Replace('ű', 'u');

            stationName = stationName.Replace(" railway station crossing", "");
            stationName = stationName.Replace(" railway station", "");
            stationName = stationName.Replace(" train station", "");
            stationName = stationName.Replace(" station", "");
            stationName = stationName.Replace(" vonatallomas", "");
            stationName = stationName.Replace(" vasutallomas", "");
            stationName = stationName.Replace(" mav pu", "");
            stationName = stationName.Replace("-", " ");

            return stationName;
        }

        /// <summary>
        /// Get station by name provided by MÁV (will be normalized)
        /// </summary>
        /// <param name="name">Station name provided by MÁV</param>
        /// <returns></returns>
        public static Station GetStation(string name)
        {
            string nn = StationNormalizeName(name);
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM stations WHERE mav_found = 1 AND norm_name = @norm_name", connection);
            cmd.Parameters.AddWithValue("@norm_name", nn);
            cmd.Prepare();
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRowsOrClose()) return null;
            reader.Read();
            Station s = new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("lon")));
            reader.Close();
            return s;
        }

        /// <summary>
        /// Get train by MySQL ID
        /// </summary>
        /// <param name="id">MySQL ID</param>
        /// <param name="update">Whether to update this train with MÁV's API</param>
        /// <returns>A Train object</returns>
        public static Train GetTrain(int id, bool update = true)
        {
            Train train = getTrainFromDB(id);
            if (train == null || !update) return train;

            JObject apiResponse = MAVAPI.RequestTrain(train.ElviraID);
            
            if (apiResponse == null) return train;

            try
            {
                train.UpdateTRAIN_API(apiResponse);
                updateTrainToDB(train);
            }
            catch (MAVAPIException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("MAVAPIException: " + e.Message);
                Console.ResetColor();
            }

            return train;
        }

        /// <summary>
        /// Get train by ElviraID
        /// </summary>
        /// <param name="elviraID">Unique ID used by MÁV</param>
        /// <param name="update">Whether to update this train with MÁV's API</param>
        /// <returns>A Train object</returns>
        public static Train GetTrainByElviraID(string elviraID, bool update = true)
        {
            Train train = getTrainFromDBByElviraID(elviraID);
            if (!update) return train;

            JObject apiResponse = MAVAPI.RequestTrain(elviraID);

            bool updated = false;
            try
            {
                if (train == null) // If the train does not exists in the DB let it be constructed from the API response
                {
                    if (apiResponse != null)
                    {
                        // This weird logic is required to not insert a train if it cannot be found/parsed (which for our purposes is the same thing)
                        train = new Train(elviraID, apiResponse);
                        updated = true;
                        insertTrainToDB(train);
                    }
                }

                if (apiResponse == null) return train;

                if (!updated)
                {
                    train.UpdateTRAIN_API(apiResponse);
                    updateTrainToDB(train);
                }
            }
            catch (MAVAPIException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }

            return train;
        }

        public static List<Train> GetTrains(bool? moving = null)
        {
            List<Train> trains = new List<Train>();
            string whereCondition = "";
            if (moving == true) whereCondition = " WHERE lat IS NOT NULL"; // lon would suffice to they are null at the same time (unless ofc. someone tinkers with the database)
            else if (moving == false) whereCondition = " WHERE lat IS NULL";

            MySqlCommand cmd = new MySqlCommand("SELECT * FROM trains" + whereCondition, connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                //TRAIN data
                int id = reader.GetInt32("id");
                string elviraID = reader.GetStringOrNull("elvira_id");
                string number = reader.GetStringOrNull("number");
                string name = reader.GetStringOrNull("name");
                string type = reader.GetStringOrNull("type");
                string numberType = reader.GetStringOrNull("number_type");
                string delayReason = reader.GetStringOrNull("delay_reason");
                string miscInfo = reader.GetStringOrDefault("misc_info", "");
                string encPolyline = reader.GetStringOrNull("enc_polyline");

                // TRAINS data
                int delay = reader.GetInt32OrDefault("delay", 0);
                Vector2 gpsPosition = reader.GetVector2OrNull("lat", "lon");
                Vector2 lastGpsPosition = reader.GetVector2OrNull("last_lat", "last_lon");
                
                trains.Add(new Train(id, elviraID, number, name, type, numberType, delay, delayReason, miscInfo, gpsPosition, lastGpsPosition, encPolyline, null));
            }

            reader.Close();

            foreach (Train train in trains)
            {
                // TRAIN data - station infos
                List<StationInfo> stations = new List<StationInfo>();
                if (train.Polyline != null) // Aka. no train data, as polyline is always supplied
                {
                    cmd = new MySqlCommand("SELECT * FROM train_stations LEFT JOIN stations ON station = stations.id WHERE train_id = @id ORDER BY number ASC", connection);
                    cmd.Parameters.AddWithValue("@id", train.ID);
                    cmd.Prepare();
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        stations.Add(new StationInfo(reader.IsDBNull(reader.GetOrdinal("station")) ? null : new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("lon"))),
                            reader.GetString("mav_name"), reader.GetInt32("int_distance"), reader.GetDouble("distance"), reader.GetInt32("position_accuracy"), reader.GetDateTime("arrive"), reader.GetDateTime("depart"),
                            reader.GetDateTime("arrive_actual"), reader.GetDateTime("depart_actual"), reader.GetBoolean("arrived"),
                            reader.IsDBNull(reader.GetOrdinal("platform")) ? null : reader.GetString("platform")));
                    }
                    reader.Close();
                }

                train.LateConstructStations(stations);
            }

            return trains;
        }

        public static List<Train> GetTrainsFilter(int[] ids, string[] elvira_ids)
        {
            List<Train> trains = new List<Train>();
            if (ids.Length == 0 && elvira_ids.Length == 0) return trains;

            string whereCondition = " WHERE ";

            if (ids.Length > 0)
            {
                whereCondition += "id in (";
                for (int i = 0; i < ids.Length; i++)
                {
                    whereCondition += $"@id_array_{i}";
                    if (i < ids.Length - 1) whereCondition += ", ";
                }
                whereCondition += ")";
            }
            
            if (elvira_ids.Length > 0)
            {
                whereCondition += $"{(whereCondition.Last() == ')' ? " OR" : "")} elvira_id in (";
                for (int i = 0; i < elvira_ids.Length; i++)
                {
                    whereCondition += $"@elvira_id_array_{i}";
                    if (i < elvira_ids.Length - 1) whereCondition += ", ";
                }
                whereCondition += ")";
            }

            MySqlCommand cmd = new MySqlCommand("SELECT * FROM trains" + whereCondition, connection);
            cmd.Parameters.AddArrayWithValue("id_array", ids);
            cmd.Parameters.AddArrayWithValue("elvira_id_array", elvira_ids);
            cmd.Prepare();
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                //TRAIN data
                int id = reader.GetInt32("id");
                string elviraID = reader.GetStringOrNull("elvira_id");
                string number = reader.GetStringOrNull("number");
                string name = reader.GetStringOrNull("name");
                string type = reader.GetStringOrNull("type");
                string numberType = reader.GetStringOrNull("number_type");
                string delayReason = reader.GetStringOrNull("delay_reason");
                string miscInfo = reader.GetStringOrDefault("misc_info", "");
                string encPolyline = reader.GetStringOrNull("enc_polyline");

                // TRAINS data
                int delay = reader.GetInt32OrDefault("delay", 0);
                Vector2 gpsPosition = reader.GetVector2OrNull("lat", "lon");
                Vector2 lastGpsPosition = reader.GetVector2OrNull("last_lat", "last_lon");

                trains.Add(new Train(id, elviraID, number, name, type, numberType, delay, delayReason, miscInfo, gpsPosition, lastGpsPosition, encPolyline, null));
            }

            reader.Close();

            foreach (Train train in trains)
            {
                // TRAIN data - station infos
                List<StationInfo> stations = new List<StationInfo>();
                if (train.Polyline != null) // Aka. no train data, as polyline is always supplied
                {
                    cmd = new MySqlCommand("SELECT * FROM train_stations LEFT JOIN stations ON station = stations.id WHERE train_id = @id ORDER BY number ASC", connection);
                    cmd.Parameters.AddWithValue("@id", train.ID);
                    cmd.Prepare();
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        stations.Add(new StationInfo(reader.IsDBNull(reader.GetOrdinal("station")) ? null : new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("lon"))),
                            reader.GetString("mav_name"), reader.GetInt32("int_distance"), reader.GetDouble("distance"), reader.GetInt32("position_accuracy"), reader.GetDateTime("arrive"), reader.GetDateTime("depart"),
                            reader.GetDateTime("arrive_actual"), reader.GetDateTime("depart_actual"), reader.GetBoolean("arrived"),
                            reader.IsDBNull(reader.GetOrdinal("platform")) ? null : reader.GetString("platform")));
                    }
                    reader.Close();
                }

                train.LateConstructStations(stations);
            }

            return trains;
        }

        /// <summary>
        /// Update with dynamic data gathered from the MÁV TRAINS API
        /// </summary>
        /// <param name="data">List of TRAINS API data</param>
        /// <param name="newTrain">Insertable train who we also update, optional</param>
        public static void UpdateDynamicData(List<TRAINSData> trainsData)
        {
            trimNullTrains();
            Dictionary<string, Train> trains = new Dictionary<string, Train>();
            List<Train> nullTrains = new List<Train>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM trains WHERE lat IS NOT NULL", connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32("id");
                string elviraID = reader.GetString("elvira_id"); // We trimmed all the nulls so there's no need for null checking

                int delay = reader.GetInt32OrDefault("delay", 0);
                Vector2 gpsPosition = reader.GetVector2OrNull("lat", "lon");
                Vector2 lastGpsPosition = reader.GetVector2OrNull("last_lat", "last_lon");

                trains.Add(elviraID, new Train(id, elviraID, null, null, null, null, delay, null, "", gpsPosition, lastGpsPosition, null, new List<StationInfo>()));
            }
            reader.Close();

            foreach (KeyValuePair<string, Train> kvp in trains)
            {
                kvp.Value.ClearPosition();
            }

            foreach (TRAINSData data in trainsData)
            {
                Train train;
                if (!trains.TryGetValue(data.ElviraID, out train))
                {
                    if (!data.ElviraID.StartsWith("_") && data.ElviraID.Length > 7) train = new Train(data.ElviraID);
                    else train = new Train(null); // These trains are sort of untrackable but as long as we have train data they may be useful

                    if (train.ElviraID == null) nullTrains.Add(train);
                    else trains.Add(train.ElviraID, train);
                }

                train.UpdateTRAINS_API(data);
                
            }

            updateTrainsAPIToDb(trains.Values.Union(nullTrains).ToList());
        }

        /// <summary>
        /// Gets a train from the database by MySQL ID
        /// </summary>
        /// <param name="id">MySQL ID</param>
        /// <returns></returns>
        private static Train getTrainFromDB(int id)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM trains WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRowsOrClose()) return null;
            reader.Read();

            //TRAIN data
            string elviraID = reader.GetStringOrNull("elvira_id");
            string number = reader.GetStringOrNull("number");
            string name = reader.GetStringOrNull("name");
            string type = reader.GetStringOrNull("type");
            string numberType = reader.GetStringOrNull("number_type");
            string delayReason = reader.GetStringOrNull("delay_reason");
            string miscInfo = reader.GetStringOrDefault("misc_info", "");
            string encPolyline = reader.GetStringOrNull("enc_polyline");

            // TRAINS data
            int delay = reader.GetInt32OrDefault("delay", 0);
            Vector2 gpsPosition = reader.GetVector2OrNull("lat", "lon");
            Vector2 lastGpsPosition = reader.GetVector2OrNull("last_lat", "last_lon");

            reader.Close();

            // TRAIN data - station infos
            List<StationInfo> stations = new List<StationInfo>();
            if (encPolyline != null) // Aka. no train data, as polyline is always supplied
            {
                cmd = new MySqlCommand("SELECT * FROM train_stations LEFT JOIN stations ON station = stations.id WHERE train_id = @id ORDER BY number ASC", connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Prepare();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    stations.Add(new StationInfo(reader.IsDBNull(reader.GetOrdinal("station")) ? null : new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("lon"))),
                        reader.GetString("mav_name"), reader.GetInt32("int_distance"), reader.GetDouble("distance"), reader.GetInt32("position_accuracy"), reader.GetDateTime("arrive"), reader.GetDateTime("depart"),
                        reader.GetDateTime("arrive_actual"), reader.GetDateTime("depart_actual"), reader.GetBoolean("arrived"),
                        reader.IsDBNull(reader.GetOrdinal("platform")) ? null : reader.GetString("platform")));
                }
                reader.Close();
            }

            Train t = new Train(id, elviraID, number, name, type, numberType, delay, delayReason, miscInfo, gpsPosition, lastGpsPosition, encPolyline, stations);
            return t;
        }

        /// <summary>
        /// Gets a train from the database by ElviraID
        /// </summary>
        /// <param name="elviraID">Unique ID used by MÁV</param>
        /// <returns></returns>
        private static Train getTrainFromDBByElviraID(string elviraID)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM trains WHERE elvira_id = @elvira_id", connection);
            cmd.Parameters.AddWithValue("@elvira_id", elviraID);
            cmd.Prepare();
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRowsOrClose()) return null;
            reader.Read();
            int id = reader.GetInt32("id");

            //TRAIN data
            string number = reader.GetStringOrNull("number");
            string name = reader.GetStringOrNull("name");
            string type = reader.GetStringOrNull("type");
            string numberType = reader.GetStringOrNull("number_type");
            string delayReason = reader.GetStringOrNull("delay_reason");
            string miscInfo = reader.GetStringOrDefault("misc_info", "");
            string encPolyline = reader.GetStringOrNull("enc_polyline");

            // TRAINS data
            int delay = reader.GetInt32OrDefault("delay", 0); 
            Vector2 gpsPosition = reader.GetVector2OrNull("lat", "lon");
            Vector2 lastGpsPosition = reader.GetVector2OrNull("last_lat", "last_lon");

            reader.Close();

            // TRAIN data - station infos
            List<StationInfo> stations = new List<StationInfo>();
            if (encPolyline != null) // Aka. no train data, as polyline is always supplied
            {
                cmd = new MySqlCommand("SELECT * FROM train_stations LEFT JOIN stations ON station = stations.id WHERE train_id = @id ORDER BY number ASC", connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Prepare();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    stations.Add(new StationInfo(reader.IsDBNull(reader.GetOrdinal("station")) ? null : new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("lon"))),
                        reader.GetString("mav_name"), reader.GetInt32("int_distance"), reader.GetDouble("distance"), reader.GetInt32("position_accuracy"), reader.GetDateTime("arrive"), reader.GetDateTime("depart"),
                        reader.GetDateTime("arrive_actual"), reader.GetDateTime("depart_actual"), reader.GetBoolean("arrived"),
                        reader.IsDBNull(reader.GetOrdinal("platform")) ? null : reader.GetString("platform")));
                }
                reader.Close();
            }

            Train t = new Train(id, elviraID, number, name, type, numberType, delay, delayReason, miscInfo, gpsPosition, lastGpsPosition, encPolyline, stations);
            return t;
        }

        /// <summary>
        /// Insert a brand new train into the database, only elvira_id will be inserted
        /// </summary>
        /// <param name="train">Train object to insert</param>
        private static void insertTrainToDB(Train train)
        {
            MySqlCommand cmd = new MySqlCommand("INSERT INTO trains (elvira_id) VALUES (@elvira_id)", connection);
            cmd.Parameters.AddWithValue("@elvira_id", train.ElviraID);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            long id = cmd.LastInsertedId;
            train.SetDBId((int)id);
        }

        /// <summary>
        /// Updates data from the TRAINS API into the database
        /// </summary>
        /// <param name="trains">List of train objects to commit into the database</param>
        private static void updateTrainsAPIToDb(List<Train> trains)
        {
            if (trains.Count == 0) return;
            string values = "";

            for (int i = 0; i < trains.Count; i++)
            {
                values += $"(@id_{i}, @elvira_id_{i}, @lat_{i}, @lon_{i}, @last_lat_{i}, @last_lon_{i}, @delay_{i})";
                if (i < trains.Count - 1) values += ", ";
            }

            MySqlCommand cmd = new MySqlCommand($"INSERT INTO trains (id, elvira_id, lat, lon, last_lat, last_lon, delay) VALUES {values} ON DUPLICATE KEY UPDATE `elvira_id` = values(`elvira_id`), `lat` = values(`lat`), `lon` = values(`lon`), `delay` = values(`delay`), `last_lat` = values(`last_lat`), `last_lon` = values(`last_lon`)", connection);
            for (int i = 0; i < trains.Count; i++)
            {
                if (trains[i].ID == -1) cmd.Parameters.AddWithValue($"@id_{i}", null);
                else cmd.Parameters.AddWithValue($"@id_{i}", trains[i].ID);
                cmd.Parameters.AddWithValue($"@elvira_id_{i}", trains[i].ElviraID);
                cmd.Parameters.AddVector2WithValue($"@lat_{i}", $"@lon_{i}", trains[i].GPSPosition);
                cmd.Parameters.AddVector2WithValue($"@last_lat_{i}", $"@last_lon_{i}", trains[i].LastGPSPosition);
                cmd.Parameters.AddWithValue($"@delay_{i}", trains[i].Delay);
            }

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Commit the changes of a train object to the database. This should only be used when updating by the TRAIN API
        /// </summary>
        /// <param name="train">Train object to update</param>
        private static void updateTrainToDB(Train train)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT enc_polyline FROM trains WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", train.ID);
            cmd.Prepare();
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            bool insertingNewData = reader.IsDBNull(reader.GetOrdinal("enc_polyline")); //updating everything not just potential changes
            reader.Close();

            if (insertingNewData)
            {
                cmd = new MySqlCommand("UPDATE trains SET number = @number, name = @name, type = @type, number_type = @number_type, delay_reason = @delay_reason, misc_info = @misc_info, enc_polyline = @enc_polyline WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", train.ID);
                cmd.Parameters.AddWithValue("@number", train.Number);
                cmd.Parameters.AddWithValue("@name", train.Name);
                cmd.Parameters.AddWithValue("@type", train.Type);
                cmd.Parameters.AddWithValue("@number_type", train.NumberType);
                cmd.Parameters.AddWithValue("@delay_reason", train.DelayReason);
                cmd.Parameters.AddWithValue("@misc_info", String.Join("\n", train.MiscInfo));
                cmd.Parameters.AddWithValue("@enc_polyline", Polyline.EncodePoints(train.Polyline.Points.ToList(), 1E5f, WebMercator.DefaultMap));
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                List<StationInfo> stations = train.Stations.ToList();
                for (int i = 0; i < stations.Count; i++)
                {
                    cmd = new MySqlCommand("INSERT INTO train_stations VALUES (@train_id, @number, @station, @mav_name, @int_distance, @distance, @position_accuracy," +
                        "@arrive, @depart, @arrive_actual, @depart_actual, @arrived, @platform)", connection);
                    cmd.Parameters.AddWithValue("@train_id", train.ID);
                    cmd.Parameters.AddWithValue("@number", i + 1);

                    if (stations[i].Station == null) cmd.Parameters.AddWithValue("@station", null);
                    else cmd.Parameters.AddWithValue("@station", stations[i].Station.ID);
                    cmd.Parameters.AddWithValue("@mav_name", stations[i].Name);
                    cmd.Parameters.AddWithValue("@int_distance", stations[i].IntDistance);
                    cmd.Parameters.AddWithValue("@distance", stations[i].Distance);
                    cmd.Parameters.AddWithValue("@position_accuracy", stations[i].PositionAccuracy == StationPositionAccuracy.Missing ? 0
                                                                        : (stations[i].PositionAccuracy == StationPositionAccuracy.IntegerPrecision ? 1 : 2));
                    cmd.Parameters.AddWithValue("@arrive", stations[i].Arrival);
                    cmd.Parameters.AddWithValue("@depart", stations[i].Departure);
                    cmd.Parameters.AddWithValue("@arrive_actual", stations[i].ExpectedArrival);
                    cmd.Parameters.AddWithValue("@depart_actual", stations[i].ExpectedDeparture);
                    cmd.Parameters.AddWithValue("@arrived", stations[i].Arrived);
                    cmd.Parameters.AddWithValue("@platform", stations[i].Platform);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                cmd = new MySqlCommand("UPDATE trains SET delay_reason = @delay_reason WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", train.ID);
                cmd.Parameters.AddWithValue("@delay_reason", train.DelayReason);
                cmd.Prepare();
                cmd.ExecuteNonQuery();


                List<StationInfo> stations = train.Stations.ToList();
                for (int i = 0; i < stations.Count; i++)
                {
                    cmd = new MySqlCommand("UPDATE train_stations SET arrive_actual = @arrive_actual, depart_actual = @depart_actual, arrived = @arrived, platform = @platform WHERE train_id = @train_id AND number = @number", connection);
                    cmd.Parameters.AddWithValue("@train_id", train.ID);
                    cmd.Parameters.AddWithValue("@number", i + 1);
                    cmd.Parameters.AddWithValue("@arrive_actual", stations[i].ExpectedArrival);
                    cmd.Parameters.AddWithValue("@depart_actual", stations[i].ExpectedDeparture);
                    cmd.Parameters.AddWithValue("@arrived", stations[i].Arrived);
                    cmd.Parameters.AddWithValue("@platform", stations[i].Platform);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Delete all trains which have a null as their elvira_id
        /// </summary>
        private static void trimNullTrains()
        {
            MySqlCommand cmd = new MySqlCommand("DELETE FROM trains WHERE elvira_id IS NULL", connection);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Get all static station information from the database (useful for printing the all)
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Station> GetAllStations()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM stations WHERE mav_found = 1 ", connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("lon")));
            }
            reader.Close();
            yield break;
        }

        /// <summary>
        /// Closes database connection
        /// </summary>
        public static void Terminate()
        {
            connection.Close();
        }
    }
}
