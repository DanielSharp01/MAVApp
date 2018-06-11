using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Name with Latitude, longitude information.
    /// See also: <seealso cref="GoogleMapsExtract.RequestPlaces"/>
    /// </summary>
    public class Station
    {
        /// <summary>
        /// ID used in the MySQL database
        /// </summary>
        public int ID
        {
            private set;
            get;
        }
        /// <summary>
        /// Name of the place
        /// </summary>
        public string Name
        {
            private set;
            get;
        }

        /// <summary>
        /// GPS Position as latitude (X) longitude (Y)
        /// </summary>
        public Vector2 GPSCoord
        {
            private set;
            get;
        }

        /// <param name="name">Name of the place</param>
        /// <param name="gpsCoord">GPS Position as latitude (X) longitude (Y)</param>
        public Station(int id, string name, Vector2 gpsCoord)
        {
            ID = id;
            Name = name;
            GPSCoord = gpsCoord;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + GPSCoord.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            Station place = obj as Station;
            if (place == null) return false;

            return place.Name == Name && place.GPSCoord == GPSCoord;
        }

        /// <summary>
        /// Adds all places in the second list to the first list if it does not already contain them (by name)
        /// </summary>
        /// <param name="places">List to add to</param>
        /// <param name="added">List to add</param>
        public static void AddPlacesTo(List<Station> places, List<Station> added)
        {
            foreach (Station place in added)
            {
                if (!places.Any(p => p.Name == place.Name))
                {
                    places.Add(place);
                }
            }
        }
    }
}
