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
    public class Station : UpdatableEntity<int>
    {
        private string name;
        /// <summary>
        /// Name of the station
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnChange();
            } 
        }

        private string normalizedName;
        /// <summary>
        /// Normalized name of the station (unique for every station)
        /// </summary>
        public string NormalizedName
        {
            get => normalizedName;
            set
            {
                normalizedName = value;
                OnChange();
            }
        }

        private Vector2 gpsCoord;
        /// <summary>
        /// GPS Position as latitude (X) longitude (Y)
        /// </summary>
        public Vector2 GPSCoord
        {
            get => gpsCoord;
            set
            {
                gpsCoord = value;
                OnChange();
            }
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
        
        protected override void InternalFill(DbDataReader reader)
        {
            Key = reader.GetInt32("id");
            Name = reader.GetString("name");
            NormalizedName = reader.GetString("norm_name");
            GPSCoord = reader.GetVector2OrNull("lat", "lon");
            Filled = true;
        }

        public override void Fill(UpdatableEntity<int> other)
        {
            if (other is Station station)
            {
                Name = station.Name;
                NormalizedName = station.NormalizedName;
                GPSCoord = station.GPSCoord;
                Filled = station.Filled;
            }
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
    public class StationNNKey : UpdatableEntity<string>
    {
        private int id;
        /// <summary>
        /// Database ID
        /// </summary>
        public int ID
        {
            get => id;
            set
            {
                id = value;
                OnChange();
            }
        }

        private string name;
        /// <summary>
        /// Name of the station
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnChange();
            }
        }

        private Vector2 gpsCoord;
        /// <summary>
        /// GPS Position as latitude (X) longitude (Y)
        /// </summary>
        public Vector2 GPSCoord
        {
            get => gpsCoord;
            set
            {
                gpsCoord = value;
                OnChange();
            }
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
            ID = reader.GetInt32("id");
            Name = reader.GetString("name");
            GPSCoord = reader.GetVector2OrNull("lat", "lon");
            Filled = true;
        }
        public override void Fill(UpdatableEntity<string> other)
        {
            if (other is StationNNKey station)
            {
                ID = station.ID;
                Name = station.Name;
                GPSCoord = station.GPSCoord;
                Filled = station.Filled;
            }
        }

        public static implicit operator Station(StationNNKey stationNN)
        {
            if (!stationNN.Filled)
                throw new InvalidOperationException("Station cannot be converted to a representation with a different key until filled.");

            return new Station(stationNN);
        }
    }
}
