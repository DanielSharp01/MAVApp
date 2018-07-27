using MAVAppBackend.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpEntities;

namespace MAVAppBackend.Model
{
    /// <summary>
    /// Station with GPS information
    /// </summary>
    public class Station : Entity<int>
    {
        /// <summary>
        /// Name of the station
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// Normalized name of the station (unique for every station)
        /// </summary>
        public string NormalizedName { private set; get; }

        /// <summary>
        /// GPS Position as latitude (X) longitude (Y)
        /// </summary>
        public Vector2 GPSCoord
        {
            private set;
            get;
        }

        /// <param name="key">Database Key</param>
        public Station(int key)
            : base(key)
        { }

        public Station(StationNNKey stationNN)
            : base(stationNN.ID)
        {
            Name = stationNN.Name;
            NormalizedName = stationNN.Key;
            GPSCoord = stationNN.GPSCoord;
        }

        /// <param name="name">Name of the station</param>
        /// <param name="norm_name">Normalized name of the station (unique for every station)</param>
        /// <param name="gpsCoord">GPS Position as latitude (X) longitude (Y)</param>
        public void Fill(string name, string normName, Vector2 gpsCoord)
        {
            Name = name;
            NormalizedName = normName;
            GPSCoord = gpsCoord;
            Filled = true;
        }
        
        protected override void InternalFill(DbDataReader reader)
        {
            Key = reader.GetInt32("id");
            Name = reader.GetString("name");
            NormalizedName = reader.GetString("norm_name");
            GPSCoord = reader.GetVector2OrNull("lat", "lon");
            Filled = true;
        }

        public static implicit operator StationNNKey(Station station)
        {
            if (!station.Filled)
                throw new InvalidOperationException("Station cannot be converted to a representation with a different key until filled.");

            return new StationNNKey(station);
        }
    }

    /// <summary>
    /// Station with GPS information with NormalizedName as database key
    /// </summary>
    public class StationNNKey : Entity<string>
    {
        /// <summary>
        /// Database ID
        /// </summary>
        public int ID { private set; get; }

        /// <summary>
        /// Name of the station
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// GPS Position as latitude (X) longitude (Y)
        /// </summary>
        public Vector2 GPSCoord
        {
            private set;
            get;
        }

        /// <param name="key">Database Key</param>
        public StationNNKey(string key)
            : base(key)
        { }

        public StationNNKey(Station station)
            : base(station.NormalizedName)
        {
            ID = station.Key;
            Name = station.Name;
            GPSCoord = station.GPSCoord;
        }

        protected override void InternalFill(DbDataReader reader)
        {
            Key = reader.GetString("norm_name");
            ID = reader.GetInt32("norm_name");
            Name = reader.GetString("name");
            GPSCoord = reader.GetVector2OrNull("lat", "lon");
            Filled = true;
        }

        public static implicit operator Station(StationNNKey stationNN)
        {
            if (!stationNN.Filled)
                throw new InvalidOperationException("Station cannot be converted to a representation with a different key until filled.");

            return new Station(stationNN);
        }
    }
}
