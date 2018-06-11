using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    public class Database
    {
        private static MySqlConnection connection;

        public static void Initialize()
        {
            connection = new MySqlConnection("Host=127.0.0.1;Database=mavapp;UserName=root;Password=mysql");
            connection.Open();
        }

        /// <summary>
        /// Normalizes a station name (removes Hungarian accents, replaces hyphens with spaces, removes redundant information such as station)
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
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM stations WHERE norm_name = @norm_name", connection);
            cmd.Parameters.AddWithValue("@norm_name", nn);
            cmd.Prepare();
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) return null;
            reader.Read();
            Station s = new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("long")));
            reader.Close();
            return s;
        }

        /// <summary>
        /// Get train by ID
        /// </summary>
        /// <param name="elviraID">Most unique ID used by MÁV</param>
        /// <returns></returns>
        public static Train GetTrain(string elviraID)
        {
            Train train = getTrainFromDB(elviraID);
            JObject apiResponse = MAVAPI.RequestTrain(elviraID);
            if (train == null) // If the train does not exists in the DB let it be constructed from the API response
            {
                if (apiResponse != null)
                {
                    train = new Train(elviraID, apiResponse);
                    UpdateDynanmicData(MAVAPI.RequestTrains(), train); // Request trains always the first time for accurate delay and positional information
                    insertTrainToDB(train);
                }
            }
            else // If the train already exists in the DB, we might still need to update it with the JSON from MÁV's API
            {
                train.UpdateTRAIN_API(apiResponse);
                updateTrainToDB(train, false);
            }

            return train;
        }

        private static Train getTrainFromDB(string elviraID)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM trains WHERE elvira_id = @elvira_id", connection);
            cmd.Parameters.AddWithValue("@elvira_id", elviraID);
            cmd.Prepare();
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();
                return null;
            }
            reader.Read();

            int id = reader.GetInt32("id");
            string number = reader.IsDBNull(reader.GetOrdinal("number")) ? null : reader.GetString("number");
            string name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString("name");
            string type = reader.IsDBNull(reader.GetOrdinal("type")) ? null : reader.GetString("type");
            string numberType = reader.IsDBNull(reader.GetOrdinal("number_type")) ? null : reader.GetString("number_type");
            string delayReason = reader.IsDBNull(reader.GetOrdinal("delay_reason")) ? null : reader.GetString("delay_reason");
            string encPolyline = reader.GetString("enc_polyline");

            Vector2 gpsPosition;
            if (reader.IsDBNull(reader.GetOrdinal("lat")) || reader.IsDBNull(reader.GetOrdinal("long")))
            {
                gpsPosition = null;
            }
            else
            {
                gpsPosition = new Vector2(reader.GetDouble("lat"), reader.GetDouble("long"));
            }

            reader.Close();

            List<StationInfo> stations = new List<StationInfo>();
            cmd = new MySqlCommand("SELECT * FROM train_stations LEFT JOIN stations ON station = stations.id WHERE train_id = @id ORDER BY number ASC", connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                stations.Add(new StationInfo(reader.IsDBNull(reader.GetOrdinal("station")) ? null : new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("long"))),
                    reader.GetString("mav_name"), reader.GetInt32("int_distance"), reader.GetDouble("distance"), reader.GetInt32("position_accuracy"), reader.GetDateTime("arrive"), reader.GetDateTime("depart"),
                    reader.GetDateTime("arrive_actual"), reader.GetDateTime("depart_actual"), reader.GetBoolean("arrived"),
                    reader.IsDBNull(reader.GetOrdinal("platform")) ? null : reader.GetString("platform")));
            }
            reader.Close();


            Train t = new Train(id, elviraID, number, name, type, numberType, delayReason, encPolyline, gpsPosition, stations);
            return t;
        }

        /// <summary>
        /// Update with dynamic data gathered from the MÁV TRAINS API
        /// </summary>
        /// <param name="data">List of TRAINS API data</param>
        /// <param name="newTrain">Insertable train who we also update, optional</param>
        public static void UpdateDynanmicData(List<TRAINSData> trainsData, Train newTrain = null)
        {
            foreach (TRAINSData data in trainsData)
            {
                if (newTrain != null && newTrain.ElviraID == data.ElviraID)
                {
                    newTrain.UpdateTRAINS_API(data);
                }
                else
                {
                    Train train = getTrainFromDB(data.ElviraID);
                    if (train != null) // If the train is known
                    {
                        train.UpdateTRAINS_API(data);
                        updateTrainToDB(train, true);
                    }
                }
            }
        }

        private static void insertTrainToDB(Train train)
        {
            MySqlCommand cmd = new MySqlCommand("INSERT INTO trains VALUES (NULL, @elvira_id, @number, @name, @type, @number_type, @delay, @delay_reason, @lat, @long, @enc_polyline)", connection);
            cmd.Parameters.AddWithValue("@elvira_id", train.ElviraID);
            cmd.Parameters.AddWithValue("@number", train.Number);
            cmd.Parameters.AddWithValue("@name", train.Name);
            cmd.Parameters.AddWithValue("@type", train.Type);
            cmd.Parameters.AddWithValue("@number_type", train.NumberType);
            if (train.GPSPosition == null)
            {
                cmd.Parameters.AddWithValue("@lat", null);
                cmd.Parameters.AddWithValue("@long", null);
            }
            else
            {
                cmd.Parameters.AddWithValue("@lat", train.GPSPosition.X);
                cmd.Parameters.AddWithValue("@long", train.GPSPosition.Y);
            }
            cmd.Parameters.AddWithValue("@delay", train.Delay);
            cmd.Parameters.AddWithValue("@delay_reason", train.DelayReason);
            cmd.Parameters.AddWithValue("@enc_polyline", Polyline.EncodePoints(train.Polyline.Points.ToList(), 1E5f, Map.DefaultMap));
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            long id = cmd.LastInsertedId;
            train.SetDBId((int)id);

            List<StationInfo> stations = train.Stations.ToList();
            for (int i = 0; i < stations.Count; i++)
            {
                cmd = new MySqlCommand("INSERT INTO train_stations VALUES (@train_id, @number, @station, @mav_name, @int_distance, @distance, @position_accuracy," +
                    "@arrive, @depart, @arrive_actual, @depart_actual, @arrived, @platform)", connection);
                cmd.Parameters.AddWithValue("@train_id", id);
                cmd.Parameters.AddWithValue("@number", i + 1);
                cmd.Parameters.AddWithValue("@station", stations[i].Station.ID);
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

        private static void updateTrainToDB(Train train, bool trainsAPI)
        {
            MySqlCommand cmd;
            if (trainsAPI)
            {
                cmd = new MySqlCommand("UPDATE trains SET lat = @lat, `long` = @long, delay = @delay WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", train.ID);
                if (train.GPSPosition == null)
                {
                    cmd.Parameters.AddWithValue("@lat", null);
                    cmd.Parameters.AddWithValue("@long", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@lat", train.GPSPosition.X);
                    cmd.Parameters.AddWithValue("@long", train.GPSPosition.Y);
                }
                cmd.Parameters.AddWithValue("@delay", train.Delay);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
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

        public static IEnumerable<Station> GetAllStations()
        {
            List<Station> stations = new List<Station>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM stations", connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return new Station(reader.GetInt32("id"), reader.GetString("name"), new Vector2(reader.GetDouble("lat"), reader.GetDouble("long")));
            }
            reader.Close();
            yield break;
        }

        public static void Terminate()
        {
            connection.Close();
        }
    }
}
