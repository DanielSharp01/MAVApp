using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public class StationMapper
    {
        private MySqlConnection connection;

        public StationMapper(MySqlConnection connection)
        {
            this.connection = connection;
        }
        

        private MySqlCommand getIdCmd = null;
        public Station GetByID(int id)
        {
            if (getIdCmd == null)
            {
                getIdCmd = new MySqlCommand("SELECT name, lat, lon FROM station_view WHERE id = @id", connection);
                getIdCmd.Prepare();
            }
            getIdCmd.Parameters.Clear();
            getIdCmd.Parameters.AddWithValue("id", id);
            MySqlDataReader reader = getIdCmd.ExecuteReader();
            if (!reader.HasRowsOrClose()) return null;
            reader.Read();

            string name = reader.GetStringOrNull("name");
            Vector2 gpsCoord = reader.GetVector2OrNull("lat", "lon");

            reader.Close();
            return new Station(id, name, gpsCoord);
        }

        private MySqlCommand getNameCmd = null;
        public Station GetByName(string stationName)
        {
            string normName = NormalizeName(stationName);
            if (getNameCmd == null)
            {
                getNameCmd = new MySqlCommand("SELECT id, name, lat, lon FROM stations WHERE norm_name = @norm_name AND mav_found = 1", connection);
                getNameCmd.Prepare();
            }
            getNameCmd.Parameters.Clear();
            getNameCmd.Parameters.AddWithValue("norm_name", normName);
            MySqlDataReader reader = getNameCmd.ExecuteReader();
            if (!reader.HasRowsOrClose()) return null;
            reader.Read();

            int id = reader.GetInt32("id");
            string name = reader.GetStringOrNull("name");
            Vector2 gpsCoord = reader.GetVector2OrNull("lat", "lon");

            reader.Close();
            return new Station(id, name, gpsCoord);
        }

        private MySqlCommand getAllCmd = null;
        public List<Station> GetAll()
        {
            if (getAllCmd == null)
            {
                getAllCmd = new MySqlCommand("SELECT id, name, lat, lon FROM stations WHERE mav_found = 1", connection);
                getAllCmd.Prepare();
            }
            
            MySqlDataReader reader = getAllCmd.ExecuteReader();
            List<Station> results = new List<Station>();

            while (reader.Read())
            {
                int id = reader.GetInt32("id");
                string name = reader.GetStringOrNull("name");
                Vector2 gpsCoord = reader.GetVector2OrNull("lat", "lon");

                results.Add(new Station(id, name, gpsCoord));
            }

            reader.Close();
            return results;
        }

        /// <summary>
        /// Normalizes a station name for comparison (removes Hungarian accents, replaces hyphens with spaces, removes redundant information such as station)
        /// </summary>
        /// <param name="stationName">Name to normalize</param>
        /// <returns>Normalized version of the same name</returns>
        public static string NormalizeName(string stationName)
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
    }
}
