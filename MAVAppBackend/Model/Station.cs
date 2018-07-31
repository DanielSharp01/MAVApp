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

        /// <param name="normName">Normalized name</param>
        public Station(string normName)
            : base(-1)
        {
            NormalizedName = normName;
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
                Key = other.Key;
                Name = station.Name;
                NormalizedName = station.NormalizedName;
                GPSCoord = station.GPSCoord;
                Filled = station.Filled;
            }
        }
    }
}
