using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Represents a Polyline stored as consecutive points
    /// </summary>
    public class Polyline
    {
        /// <summary>
        /// Points representing the polyline
        /// </summary>
        private List<Vector2> points = new List<Vector2>();

        /// <summary>
        /// Map projection of the points
        /// </summary>
        public Map Map
        {
            private set;
            get;
        }

        /// <summary>
        /// Points representing the polyline
        /// </summary>
        public IEnumerable<Vector2> Points
        {
            get
            {
                foreach (Vector2 p in points)
                {
                    yield return p;
                }
                yield break;
            }
        }

        /// <param name="points">Points representing the polyline</param>
        /// <param name="map">Map projection of the points</param>
        public Polyline(List<Vector2> points, Map map)
        {
            foreach (Vector2 p in points)
            {
                this.points.Add(p);
            }
            this.Map = map;
        }

        /// <summary>
        /// Gets the points representing this polyline in a new projection
        /// <param name="map">New projection</param>
        /// </summary>
        public IEnumerable<Vector2> ReprojectPoints(Map map)
        {
            foreach (Vector2 p in points)
            {
                yield return p;
            }
            yield break;
        }

        /// <summary>
        /// <para>Gets a point on the line at a specific distance from the start</para>
        /// <para>Note: Distance means the distance of line segments which are before the point plus the distance on the specific line segment where the point is</para>
        /// </summary>
        /// <param name="km">Distance from the start in kilometers</param>
        /// <returns>Point at a specific distance away from start</returns>
        public Vector2 GetPoint(double km)
        {
            if (km < 0) return points.First();

            double kmpp = Map.MeterPerWebMercUnit() / 1000;

            for (int i = 0; i < points.Count - 1; i++)
            {
                double pdist = (points[i] - points[i + 1]).Length * kmpp;
                if (km < pdist)
                {
                    return Vector2.Lerp(points[i], points[i + 1], km / pdist);
                }
                else km -= pdist;
            }

            return points.Last();
        }

        /// <summary>
        /// <para>Gets the distance on the line of a point from the start when projected onto the closest line segment</para>
        /// <para>Note: Distance means the distance of line segments which are before the point plus the distance on the specific line segment where the point is</para>
        /// </summary>
        /// <param name="point">Point to be projected</param>
        /// <param name="map">Map projection to take into account</param>
        /// <returns>Distance from the start</returns>
        public double GetProjectedDistance(Vector2 point)
        {
            double kmpp = Map.MeterPerWebMercUnit() / 1000;

            int bestIndex = -1;
            double bestProj = 0;
            double bestDist = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i + 1] - points[i];
                Vector2 b = point - points[i];
                double proj = a.Dot(b) / a.LengthSquared;
                if (proj < 0) proj = 0;
                else if (proj > 1) proj = 1;

                double dist = (point - (points[i] + a * proj)).Length * kmpp;

                if (bestIndex == -1 || dist < bestDist)
                {
                    bestIndex = i;
                    bestProj = proj;
                    bestDist = dist;
                }

            }

            double km = 0;
            for (int i = 0; i < bestIndex; i++)
            {
                km += (points[i + 1] - points[i]).Length * kmpp;
            }

            km += bestProj * (points[bestIndex + 1] - points[bestIndex]).Length * kmpp;

            return km;
        }

        /// <summary>
        /// <para>Decodes a polyline from string into points as GPS Position as latitude (X) longitude (Y)</para>
        /// <para>Source: https://github.com/mapbox/polyline/blob/master/src/polyline.js</para>
        /// </summary>
        /// <param name="polyline">Encoded polyline</param>
        /// <param name="precisionFactor">Precision factor of the encoded polyline. Same as the one you encoded with if you encoded yourself.</param>
        /// <param name="map">Map projection to convert into</param>
        /// <returns>Points of the polyline as GPS Position as latitude (X) longitude (Y)</returns>
        public static List<Vector2> DecodePoints(string polyline, double precisionFactor, Map map)
        {
            List<Vector2> points = new List<Vector2>();
            int latitude = 0;
            int longitude = 0;
            for (int i = 0; i < polyline.Length;)
            {
                int b;
                int shift = 0;
                int result = 0;
                do
                {
                    b = polyline[i++] - 63;
                    result |= (b & 31) << shift;
                    shift += 5;
                }
                while (32 <= b);
                latitude += (result & 1) > 0 ? ~(result >> 1) : result >> 1;

                result = shift = 0;
                do
                {
                    b = polyline[i++] - 63;
                    result |= (b & 31) << shift;
                    shift += 5;
                }
                while (32 <= b);
                longitude += (result & 1) > 0 ? ~(result >> 1) : result >> 1;
                points.Add(map.FromLatLon(new Vector2(latitude / precisionFactor, longitude / precisionFactor)));
            }

            return points;
        }

        /// <summary>
        /// <para>Encodes a polyline into string from points as GPS Position as latitude (X) longitude (Y)</para>
        /// <para>Source: https://github.com/mapbox/polyline/blob/master/src/polyline.js</para>
        /// </summary>
        /// <param name="points">Points as GPS Position as latitude (X) longitude (Y)</param>
        /// <param name="precisionFactor">Precision factor to encode with.</param>
        /// <param name="map">Map projection of the points</param>
        /// <returns>Encoded polyline</returns>
        public static string EncodePoints(List<Vector2> points, double precisionFactor, Map map)
        {
            List<Vector2> encodablePoints = new List<Vector2>();

            foreach (Vector2 point in points)
            {
                encodablePoints.Add(map.ToLatLon(point));
            }

            double factor = 1E5f;
            string output = encodeHelper(points[0].X, 0, factor) + encodeHelper(points[0].Y, 0, factor);

            for (var i = 1; i < points.Count; i++)
            {
                Vector2 current = points[i], previous = points[i - 1];
                output += encodeHelper(current.X, previous.X, precisionFactor);
                output += encodeHelper(current.Y, previous.Y, precisionFactor);
            }

            return output;
        }

        /// <summary>
        /// <para>Helper method for encoding polylines</para>
        /// <para>Source: https://github.com/mapbox/polyline/blob/master/src/polyline.js</para>
        /// </summary>
        /// <param name="current">Current coordinate.X or coordinate.Y (Latitude or Longitude)</param>
        /// <param name="previous">Previous coordinate.X or coordinate.Y (Latitude or Longitude)</param>
        /// <param name="precisionFactor">Precision factor to encode with.</param>
        /// <returns></returns>
        private static string encodeHelper(double current, double previous, double precisionFactor)
        {
            int coordinate = pythonRound(current * precisionFactor) - pythonRound(previous * precisionFactor);
            coordinate <<= 1;
            if (current - previous < 0)
            {
                coordinate = ~coordinate;
            }

            string output = "";
            while (coordinate >= 32)
            {
                output += (char)((32 | (coordinate & 31)) + 63);
                coordinate >>= 5;
            }
            output += (char)(coordinate + 63);
            return output;
        }

        /// <summary>
        /// Python's rounding function used by the encoding algorithms
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <returns>Rounded value</returns>
        private static int pythonRound(double value)
        {
            return (int)Math.Floor(Math.Abs(value) + 0.5) * (value >= 0 ? 1 : -1);
        }
    }
}
