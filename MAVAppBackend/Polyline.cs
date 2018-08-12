using System;
using System.Collections.Generic;
using System.Linq;

namespace MAVAppBackend
{
    /// <summary>
    /// Represents a Polyline stored as consecutive points
    /// </summary>
    public class Polyline
    {
        /// <summary>
        /// Points representing the polyline as latitude, longitude
        /// </summary>
        private List<Vector2> points = new List<Vector2>();

        /// <summary>
        /// NVectors (a form of encoding geographic position) for better distance calculation
        /// </summary>
        private List<Vector2> projectedPoints = new List<Vector2>();

        /// <summary>
        /// Points representing the polyline as latitude, longitude
        /// </summary>
        public IReadOnlyList<Vector2> Points;

        /// <summary>
        /// Points representing the polyline in the WebMercator.Default projection
        /// </summary>
        public IReadOnlyList<Vector2> ProjectedPoints;

        /// <param name="points">Points representing the polyline as latitude, longitude</param>
        public Polyline(IEnumerable<Vector2> points)
        {
            this.points.AddRange(points);
            Points = this.points.AsReadOnly();

            projectedPoints.AddRange(this.points.Select(p => WebMercator.Default.FromLatLon(p)));
            ProjectedPoints = projectedPoints.AsReadOnly();
        }

        /// <summary>
        /// <para>Gets a point on the line as latitude, longitude at a specific distance from the start</para>
        /// <para>Note: Distance means the distance of line segments which are before the point plus the distance on the specific line segment where the point is</para>
        /// </summary>
        /// <param name="km">Distance from the start in kilometers</param>
        /// <returns>Point at a specific distance away from start</returns>
        public Vector2 GetPoint(double km)
        {
            if (km < 0) return points.First();

            double kmpu = WebMercator.Default.MeterPerUnit() / 1000;

            for (int i = 0; i < projectedPoints.Count - 1; i++)
            {
                double pdist = (projectedPoints[i] - projectedPoints[i + 1]).Length * kmpu;
                if (km < pdist)
                {
                    return WebMercator.Default.ToLatLon(Vector2.Lerp(projectedPoints[i], projectedPoints[i + 1], km / pdist));
                }
                else km -= pdist;
            }

            return points.Last(); // We don't have to convert back yay
        }

        /// <summary>
        /// <para>Gets the distance on the line of a point from the start when projected onto the closest line segment.</para>
        /// <para>Note: Distance means the distance of line segments which are before the point plus the distance on the specific line segment where the point is</para>
        /// </summary>
        /// <param name="point">Point to be projected as latitude, longitude</param>
        /// <param name="distanceLimit">The distance limit with which a point is still considered projectable</param>
        /// <returns>Distance from the start if projectable, null otherwise</returns>
        public double? GetProjectedDistance(Vector2 point, double distanceLimit)
        {
            point = WebMercator.Default.FromLatLon(point);
            double kmpp = WebMercator.Default.MeterPerUnit() / 1000;

            int bestIndex = -1;
            double bestProj = 0;
            double bestDist = 0;

            for (int i = 0; i < projectedPoints.Count - 1; i++)
            {
                Vector2 a = projectedPoints[i + 1] - projectedPoints[i];
                Vector2 b = point - projectedPoints[i];
                double proj = a.Dot(b) / a.LengthSquared;

                if (proj < 0) proj = 0;
                else if (proj > 1) proj = 1;

                double dist = (point - (projectedPoints[i] + a * proj)).Length * kmpp;

                if ((bestIndex == -1 || dist < bestDist) && dist < distanceLimit)
                {
                    bestIndex = i;
                    bestProj = proj;
                    bestDist = dist;
                }
            }

            // The point could not be projected
            if (bestIndex == -1)
                return null;

            double km = 0;
            for (int i = 0; i < bestIndex; i++)
            {
                km += (projectedPoints[i + 1] - projectedPoints[i]).Length * kmpp;
            }

            km += bestProj * (projectedPoints[bestIndex + 1] - projectedPoints[bestIndex]).Length * kmpp;

            return km;
        }

        /// <summary>
        /// Returns a polyline segment between point a and point b
        /// </summary>
        /// <param name="a">Point a</param>
        /// <param name="b">Point b</param>
        /// <param name="distanceLimit">The distance limit with which a point is still considered projectable</param>
        /// <returns>If a or b are projectable the segment between them as a polyline, if not null</returns>
        public Polyline SegmentBetween(Vector2 a, Vector2 b, double distanceLimit)
        {
            List<Vector2> segmentPoints = new List<Vector2>();

            double? aDist = GetProjectedDistance(a, distanceLimit);
            double? bDist = GetProjectedDistance(b, distanceLimit);

            if (!aDist.HasValue || !bDist.HasValue) return null;

            if (aDist > bDist)
            {
                Vector2 tmp = a;
                a = b;
                b = tmp;

                double? tmpDist = aDist;
                aDist = bDist;
                bDist = tmpDist;
            }

            double kmpu = WebMercator.Default.MeterPerUnit() / 1000;
            double totalDistance = 0;
            for (int i = 0; i < projectedPoints.Count - 1; i++)
            {
                double pdist = (projectedPoints[i] - projectedPoints[i + 1]).Length * kmpu;

                if (aDist <= totalDistance + pdist && segmentPoints.Count == 0)
                {
                    segmentPoints.Add(a);
                }
                else if (totalDistance + pdist < bDist && segmentPoints.Count > 0)
                {
                    segmentPoints.Add(points[i + 1]);
                }
                else if (segmentPoints.Count > 0)
                {
                    segmentPoints.Add(b);
                }

                totalDistance += pdist;
            }

            return new Polyline(segmentPoints);
        }

        /// <summary>
        /// <para>Decodes a polyline from string into points as GPS Position as latitude (X) longitude (Y)</para>
        /// <para>Source: https://github.com/mapbox/polyline/blob/master/src/polyline.js</para>
        /// </summary>
        /// <param name="polyline">Encoded polyline</param>
        /// <param name="precisionFactor">Precision factor of the encoded polyline. Same as the one you encoded with if you encoded yourself.</param>
        /// <returns>Points of the polyline as latitude, longitude</returns>
        public static List<Vector2> DecodePoints(string polyline, double precisionFactor)
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
                points.Add(p);
            }

            return points;
        }

        /// <summary>
        /// <para>Encodes a polyline into string from points as GPS Position as latitude (X) longitude (Y)</para>
        /// <para>Source: https://github.com/mapbox/polyline/blob/master/src/polyline.js</para>
        /// </summary>
        /// <param name="points">Points as latitude, longitude</param>
        /// <param name="precisionFactor">Precision factor to encode with.</param>
        /// <returns>Encoded polyline</returns>
        public static string EncodePoints(IReadOnlyList<Vector2> points, double precisionFactor)
        {
            string output = encodeHelper(points[0].X, 0, precisionFactor) + encodeHelper(points[0].Y, 0, precisionFactor);

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
