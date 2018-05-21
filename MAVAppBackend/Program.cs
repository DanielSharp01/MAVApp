using System;
using System.Collections.Generic;
using System.Text;

namespace MAVAppBackend
{
    public class MavData
    {
        public string Name;
        public int Distance;

        public MavData(string name, int distance)
        {
            Name = name;
            Distance = distance;
        }
    }
    public class Program
    {
        public static string encodedTestPolyline = "ag|aHk_awB|^kDhB]bZeCzDk@xDoAtDyBfD{C|CcEbwOenVl`@em@viAmgBrFaK|~DiiIvGoLbE{FhE_FnH{Gjk@}e@z]sXnlAmcAbkDquCnx[c`WhQgMtwAi`AnGiDb_@eVlz@qk@fyRmpMfEcDtDmEpAsBvAyCrB{F|AeHt@yF^aGLiGEiGUoFm@qG}@{FmAkFuBmGyAcDkHmLwBgEaOeU}DqF";

        public static PlacesData GetBestStation(List<PlacesData> places, string stationName)
        {
            PlacesData best = null;
            int bestDist = 0;
            foreach (PlacesData data in places)
            {
                int dist = Functions.LevehnsteinDist(data.Name, stationName);
                if (best == null || dist < bestDist)
                {
                    bestDist = dist;
                    best = data;
                }
                Console.WriteLine(data.Name);
            }

            return best;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            List<Vector2> points = Polyline.DecodePoints(encodedTestPolyline, 1E5f);
            List<Vector2> drawablePoints = new List<Vector2>();

            List<MavData> stations = new List<MavData>();
            stations.Add(new MavData("Hatvan", 0));
            stations.Add(new MavData("Jászfényszaru", 11));
            stations.Add(new MavData("Pusztamonostor", 15));
            stations.Add(new MavData("Jászberény", 26));
            stations.Add(new MavData("Meggyespele", 33));
            stations.Add(new MavData("Portelek", 36));
            stations.Add(new MavData("Jászboldogháza-Jánoshida", 42));
            stations.Add(new MavData("Újszász", 52));
            stations.Add(new MavData("Szolnok", 68));

            Vector2 center = Map.LatLonToWebMerc(Map.Center);

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = Map.LatLonToWebMerc(points[i]) - center;
                drawablePoints.Add(Map.WebMercToScaledWebMerc(points[i]));
            }

            Polyline polyline = new Polyline(points);
            Polyline drawablePolyline = new Polyline(drawablePoints);

            SVGStream svg = new SVGStream(@"C:\Users\DanielSharp\Desktop\asd.svg", 1920, 1080);
            svg.DrawPolyline(drawablePolyline, new Vector2(1920 / 2, 1080 / 2), "black", 1);

            foreach (MavData station in stations)
            {
                Vector2 mavPoint = polyline.GetPoint(station.Distance);
                PlacesData data = GetBestStation(GoogleMaps.RequestPlaces(Map.WebMercToLatLon(mavPoint + center)), station.Name);
                double dist = polyline.GetProjectedDistance(Map.LatLonToWebMerc(data.GPSCoord) - center);
                svg.DrawPoint(Map.WebMercToScaledWebMerc(polyline.GetPoint(dist)), new Vector2(1920 / 2, 1080 / 2), "green", 2);
            }
            svg.Close();
        }
    }
}
