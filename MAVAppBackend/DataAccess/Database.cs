using System;
using MAVAppBackend.EntityMappers;
using SharpEntities.MySql;

namespace MAVAppBackend.DataAccess
{
    public class Database : IDisposable
    {
        private MySqlDatabaseConnection connection;

        private static Database instance;
        public static Database Instance => instance ?? (instance = new Database(new MySqlDatabaseConnection("Host=127.0.0.1;Database=mavapp;UserName=root;Password=mysql")));

        private Database(MySqlDatabaseConnection connection)
        {
            this.connection = connection;
        }

        #region Mappers
        private StationMapper stationMapper;
        public StationMapper StationMapper => stationMapper ?? (stationMapper = new StationMapper(connection));

        private TrainMapper trainMapper;
        public TrainMapper TrainMapper => trainMapper ?? (trainMapper = new TrainMapper(connection));

        private TrainInstanceMapper trainInstanceMapper;
        public TrainInstanceMapper TrainInstanceMapper => trainInstanceMapper ?? (trainInstanceMapper = new TrainInstanceMapper(connection));

        private TrainStationMapper trainStationMapper;
        public TrainStationMapper TrainStationMapper => trainStationMapper ?? (trainStationMapper = new TrainStationMapper(connection));

        private TrainInstanceStationMapper trainInstanceStationMapper;
        public TrainInstanceStationMapper TrainInstanceStationMapper => trainInstanceStationMapper ?? (trainInstanceStationMapper = new TrainInstanceStationMapper(connection));

        private TraceMapper traceMapper;
        public TraceMapper TraceMapper => traceMapper ?? (traceMapper = new TraceMapper(connection));
        #endregion

        public void Dispose()
        {
            connection?.Close();
            connection = null;
        }

        /// <summary>
        /// Normalizes a station name (removes Hungarian accents, replaces hyphens with spaces, removes redundant information such as "station")
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

    }
}
