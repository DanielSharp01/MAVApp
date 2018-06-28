using MAVAppBackend.DataAccess;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public class Database : IDisposable
    {
        private MySqlConnection connection;

        private static Database instance;
        public static Database Instance { get => instance ?? (instance = new Database(new MySqlConnection("Host=127.0.0.1;Database=mavapp;UserName=root;Password=mysql"))); }

        private Database(MySqlConnection connection)
        {
            this.connection = connection;
            connection.Open();
        }

        // Mappers
        private StationMapper stationMapper;
        public StationMapper StationMapper { get => stationMapper ?? (stationMapper = new StationMapper(connection)); }

        private LineMapper lineMapper;
        public LineMapper LineMapper { get => lineMapper ?? (lineMapper = new LineMapper(connection)); }

        private StationLineMapper stationLineMapper;
        public StationLineMapper StationLineMapper { get => stationLineMapper ?? (stationLineMapper = new StationLineMapper(connection)); }

        public void Dispose()
        {
            if (connection != null) connection.Close();
            connection = null;
        }
    }
}
