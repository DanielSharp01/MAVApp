using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    public class GeoMath
    {
        /// <summary>
        /// Radius of the Earth in kms
        /// </summary>
        public const int EARTH_RADIUS = 6371;

        /// <summary>
        /// Radians to degrees
        /// </summary>
        /// <param name="rad">Radians</param>
        public static double Deg(double rad)
        {
            return rad / Math.PI * 180.0;
        }

        /// <summary>
        /// Degrees to radians
        /// </summary>
        /// <param name="deg">Degrees</param>
        public static double Rad(double deg)
        {
            return deg / 180.0 * Math.PI;
        }

        /// <summary>
        /// Converts from Latitude, Longitude pair to NVector
        /// </summary>
        /// <param name="nvec">Latitude, longitude representation of Geographic position</param>
        public static Vector3 LatLonToNVector(Vector2 latLon)
        {
            return new Vector3(Math.Cos(Rad(latLon.X)) * Math.Cos(Rad(latLon.Y)), Math.Cos(Rad(latLon.X)) * Math.Sin(Rad(latLon.Y)), Math.Sin(Rad(latLon.X)));
        }

        /// <summary>
        /// Converts from NVector to Latitude, Longitude pair
        /// </summary>
        /// <param name="nvec">NVector representation of Geographic position</param>
        public static Vector2 NVectorToLatLon(Vector3 nvec)
        {
            return new Vector2(Math.Atan2(nvec.Z, Math.Sqrt(nvec.X * nvec.X + nvec.Y * nvec.Y)), Math.Atan2(nvec.Y, nvec.X));
        }

        /// <summary>
        /// Measures the geodesic distance between two points expressed as NVectors
        /// </summary>
        public static double DistanceBetweenNVectors(Vector3 a, Vector3 b)
        {
            return EARTH_RADIUS * Math.Atan2(a.Cross(b).Length, a.Dot(b));
        }
    }
}
