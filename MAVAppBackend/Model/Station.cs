using MAVAppBackend.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend.Model
{
    /// <summary>
    /// Station with GPS information
    /// </summary>
    public class Station : Entity
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

        /// <param name="id">Database ID</param>
        public Station(int id)
            : base(id)
        { }

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

        /// <param name="norm_name">Normalized name of the station (unique for every station)</param>
        public Station(string normName)
            : base(-1)
        {
            NormalizedName = normName;
        }

        /// <param name="id">Database ID</param>
        /// <param name="name">Name of the station</param>
        /// <param name="gpsCoord">GPS Position as latitude (X) longitude (Y)</param>
        public void Fill(int id, string name, Vector2 gpsCoord)
        {
            ID = id;
            Name = name;
            GPSCoord = gpsCoord;
            Filled = true;
        }
    }
}
