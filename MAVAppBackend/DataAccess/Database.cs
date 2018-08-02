using MAVAppBackend.EntityMappers;
using System;
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
        #endregion

        public void Dispose()
        {
            connection?.Close();
            connection = null;
        }
    }
}
