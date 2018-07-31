using MAVAppBackend.EntityMappers;
using MySql.Data.MySqlClient;
using SharpEntities;
using System;
using System.Collections.Generic;
using MAVAppBackend.Model;

namespace MAVAppBackend.DataAccess
{
    public class Database : IDisposable
    {
        private MySqlConnection connection;

        private static Database instance;
        public static Database Instance => instance ?? (instance = new Database(new MySqlConnection("Host=127.0.0.1;Database=mavapp;UserName=root;Password=mysql")));

        private Database(MySqlConnection connection)
        {
            this.connection = connection;
            connection.Open();
        }

        // Mappers
        private StationMapper stationMapper;
        public StationMapper StationMapper => stationMapper ?? (stationMapper = new StationMapper(connection));

        private TrainEntityMapper trainMapper;
        public TrainEntityMapper TrainMapper => trainMapper ?? (trainMapper = new TrainEntityMapper(connection));

        public void Dispose()
        {
            connection?.Close();
            connection = null;
        }
    }
}
