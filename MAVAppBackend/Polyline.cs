﻿using System;
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
        public Polyline(List<Vector2> points)
        {
            foreach (Vector2 p in points)
            {
                this.points.Add(p);
            }
        }

        /// <summary>
        /// <para>Gets a point on the line at a specific distance from the start</para>
        /// <para>Note: Distance means the distance of line segments which are before the point plus the distance on the specific line segment where the point is</para>
        /// </summary>
        /// <param name="km">Distance from the start in kilometers</param>
        /// <param name="map">The map projection to take into account</param>
        /// <returns>Point at a specific distance away from start</returns>
        public Vector2 GetPoint(double km, Map map)
        {
            if (km < 0) return points.First();

            double kmpp = map.MeterPerWebMercUnit() / 1000;

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

        static int it = 0;

        /// <summary>
        /// <para>Gets the distance on the line of a point from the start when projected onto the closest line segment</para>
        /// <para>Note: Distance means the distance of line segments which are before the point plus the distance on the specific line segment where the point is</para>
        /// </summary>
        /// <param name="point">Point to be projected</param>
        /// <param name="map">The map projection to take into account</param>
        /// <param name="distanceLimit">The distance limit with which a point is still considered projectable</param>
        /// <returns>Distance from the start if projectable, NAN otherwise</returns>
        public double GetProjectedDistance(Vector2 point, Map map, double distanceLimit)
        {
            double kmpp = map.MeterPerWebMercUnit() / 1000;

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

                if ((bestIndex == -1 || dist < bestDist) && dist < distanceLimit)
                {
                    bestIndex = i;
                    bestProj = proj;
                    bestDist = dist;
                }
            }

            // The point could not be projected
            if (bestIndex == -1)
                return double.NaN;

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
        /// <param name="map">Map projection to convert into (if null than points are returned as Latitude (X), Longitude (Y) coordinates)</param>
        /// <returns>Points of the polyline in the projection specified by the map parameter</returns>
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
                Vector2 p = new Vector2(latitude / precisionFactor, longitude / precisionFactor);
                if (map != null) p = map.FromLatLon(p);
                points.Add(p);
            }

            return points;
        }

        /// <summary>
        /// <para>Encodes a polyline into string from points as GPS Position as latitude (X) longitude (Y)</para>
        /// <para>Source: https://github.com/mapbox/polyline/blob/master/src/polyline.js</para>
        /// </summary>
        /// <param name="points">Points in the projection specified by the map parameter</param>
        /// <param name="precisionFactor">Precision factor to encode with.</param>
        /// <param name="map">Map projection of the points (if null than points are treated as Latitude (X), Longitude (Y) coordinates)</param>
        /// <returns>Encoded polyline</returns>
        public static string EncodePoints(List<Vector2> points, double precisionFactor, Map map)
        {
            List<Vector2> encodablePoints = new List<Vector2>();

            if (map != null)
            {
                foreach (Vector2 point in points)
                {
                    encodablePoints.Add(map.ToLatLon(point));
                }
            }
            else
            {
                encodablePoints = points;
            }

            string output = encodeHelper(encodablePoints[0].X, 0, precisionFactor) + encodeHelper(encodablePoints[0].Y, 0, precisionFactor);

            for (var i = 1; i < encodablePoints.Count; i++)
            {
                Vector2 current = encodablePoints[i], previous = encodablePoints[i - 1];
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
