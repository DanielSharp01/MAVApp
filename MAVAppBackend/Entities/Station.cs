using MAVAppBackend.DataAccess;
using SharpEntities;
using System.Data.Common;

namespace MAVAppBackend.Entities
{
    /// <summary>
    /// Station with GPS information
    /// </summary>
    public class Station : Entity<int>
    {
        /// <summary>
        /// Name of the station
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Normalized name of the station (unique for every station)
        /// </summary>
        public string NormalizedName { get; set; }

        /// <summary>
        /// GPS Position as latitude (X) longitude (Y)
        /// </summary>
        public Vector2 GPSCoord { get; set; }
        
        public override void Fill(DbDataReader reader)
        {
            Key = reader.GetInt32("id");
            Name = reader.GetString("name");
            NormalizedName = reader.GetString("norm_name");
            GPSCoord = reader.GetVector2OrNull("lat", "lon");
            Filled = true;
        }

        public override void Fill(Entity<int> other)
        {
            if (!(other is Station station)) return;

            Key = other.Key;
            Name = station.Name;
            NormalizedName = station.NormalizedName;
            GPSCoord = station.GPSCoord;
            Filled = station.Filled;
        }
    }
}
