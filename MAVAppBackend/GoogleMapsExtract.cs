using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Name with Latitude, longitude information.
    /// See also: <seealso cref="GoogleMapsExtract.RequestPlaces"/>
    /// </summary>
    public class PlacesData
    {
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
        public PlacesData(string name, Vector2 gpsCoord)
        {
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
            PlacesData place = obj as PlacesData;
            if (place == null) return false;

            return place.Name == Name && place.GPSCoord == GPSCoord;
        }

        /// <summary>
        /// Adds all places in the second list to the first list if it does not already contain them (by name)
        /// </summary>
        /// <param name="places">List to add to</param>
        /// <param name="added">List to add</param>
        public static void AddPlacesTo(List<PlacesData> places, List<PlacesData> added)
        {
            foreach (PlacesData place in added)
            {
                if (!places.Any(p => p.Name == place.Name))
                {
                    places.Add(place);
                }
            }
        }
    }

    class PlaceAPIException : Exception
    {
        public PlaceAPIException(string message) : base(message)
        { }
    }


    /// <summary>
    /// Reponsible for extracting GPS data from Google Maps
    /// </summary>
    public class GoogleMapsExtract
    {
        /// <summary>
        /// Your own GooglePlaces API key
        /// Note: You do have to set this up in your environment variables.
        /// </summary>
        private static string GooglePlacesAPIKey = Environment.GetEnvironmentVariable("PlacesAPIKey", EnvironmentVariableTarget.User);
        
        /// <summary>
        /// Request GooglePlaces API for train stations in a given position
        /// </summary>
        /// <param name="position">GPS Position as latitude (X) longitude (Y)</param>
        /// <param name="radius">Radius of search (parameter of the API)</param>
        /// <returns>List of Google Places data containing name and coordinate of found stations</returns>
        public static List<PlacesData> RequestPlaces(Vector2 position, int radius)
        {
            List<PlacesData> ret = new List<PlacesData>();
            HttpWebRequest request = WebRequest.CreateHttp("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + position.ToString() + "&radius=" + radius + "&type=train_station&key=" + GooglePlacesAPIKey);
            request.Method = "GET";
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        JObject whole = JObject.Parse(json);
                        string status = whole["status"].ToString();
                        if (status == "OK" || status == "ZERO_RESULTS")
                        {
                            JArray results = whole["results"] as JArray;
                            foreach (JObject place in results)
                            {
                                ret.Add(new PlacesData(place["name"].ToString(),
                                                       new Vector2(place["geometry"]["location"]["lat"].ToString(), place["geometry"]["location"]["lng"].ToString())));
                            }
                        }
                        else
                        {
                            throw new PlaceAPIException("Places API is unavailable. Status code: " + status);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                throw new PlaceAPIException("Places API is unavailable.");
            }

            return ret;
        }

        /// <summary>
        /// Scans Hungary (and near the border) for all it's train stations with the Google Places API
        /// </summary>
        /// <param name="requestCnt">Out parameter: requests made towards the API</param>
        /// <param name="lostData">GPS coordinates Latitude (X), Longitude (Y) where there can be stations not returned by the request (more than 20)</param>
        /// <returns>A list of place names and coordinates for every train station in the area</returns>
        public static List<PlacesData> ScanCountry(out int requestCnt, List<Vector2> lostData)
        {
            List<PlacesData> ret = new List<PlacesData>();
            int cnt = 0;

            double north1 = 48.029010, south1 = 45.728777, west1 = 16.068758, east1 = 18.720120; //First bounding box
            double north2 = 48.347756, south2 = 46.076577, west2 = 18.720120, east2 = 20.282168; //Second bounding box
            double north3 = 48.607958, south3 = 46.115736, west3 = 20.282168, east3 = 21.767767; //Third bounding box
            double north4 = 48.444897, south4 = 47.068252, west4 = 21.767767, east4 = 22.931403; //Fourth bounding box
            double north5 = 47.622261, south5 = 47.353142, west5 = 18.906707, east5 = 19.347513; //Budapest bounding box

            // 5km step by default
            double step = 4.0 / 3.0 * 5000 / Map.DefaultMap.MeterPerWebMercUnit();

            int expectedRows1 = (int)((Map.DefaultMap.FromLatLon(new Vector2(north1, east1)).X - Map.DefaultMap.FromLatLon(new Vector2(north1, west1)).X) / step) + 1;
            int expectedColumns1 = (int)((Map.DefaultMap.FromLatLon(new Vector2(south1, west1)).Y - Map.DefaultMap.FromLatLon(new Vector2(north1, west1)).Y) / step) + 1;
            int expectedReqs1 = expectedRows1 * expectedColumns1;

            int expectedRows2 = (int)((Map.DefaultMap.FromLatLon(new Vector2(north2, east2)).X - Map.DefaultMap.FromLatLon(new Vector2(north2, west2)).X) / step) + 1;
            int expectedColumns2 = (int)((Map.DefaultMap.FromLatLon(new Vector2(south2, west2)).Y - Map.DefaultMap.FromLatLon(new Vector2(north2, west2)).Y) / step) + 1;
            int expectedReqs2 = expectedRows2 * expectedColumns2;

            int expectedRows3 = (int)((Map.DefaultMap.FromLatLon(new Vector2(north3, east3)).X - Map.DefaultMap.FromLatLon(new Vector2(north3, west3)).X) / step) + 1;
            int expectedColumns3 = (int)((Map.DefaultMap.FromLatLon(new Vector2(south3, west3)).Y - Map.DefaultMap.FromLatLon(new Vector2(north3, west3)).Y) / step) + 1;
            int expectedReqs3 = expectedRows3 * expectedColumns3;

            int expectedRows4 = (int)((Map.DefaultMap.FromLatLon(new Vector2(north4, east4)).X - Map.DefaultMap.FromLatLon(new Vector2(north4, west4)).X) / step) + 1;
            int expectedColumns4 = (int)((Map.DefaultMap.FromLatLon(new Vector2(south4, west4)).Y - Map.DefaultMap.FromLatLon(new Vector2(north4, west4)).Y) / step) + 1;
            int expectedReqs4 = expectedRows4 * expectedColumns4;

            // 2km step for Budapest
            int expectedRows5 = (int)((Map.DefaultMap.FromLatLon(new Vector2(north5, east5)).X - Map.DefaultMap.FromLatLon(new Vector2(north5, west5)).X) / 2000) + 1;
            int expectedColumns5 = (int)((Map.DefaultMap.FromLatLon(new Vector2(south5, west5)).Y - Map.DefaultMap.FromLatLon(new Vector2(north5, west5)).Y) / 2000) + 1;
            int expectedReqs5 = expectedRows5 * expectedColumns5;

            int expectedReqs = expectedReqs1 + expectedReqs2 + expectedReqs3 + expectedReqs4 + expectedReqs5;

            // First bounding box
            int cntInc = 0;
            PlacesData.AddPlacesTo(ret, ScanBoundingBox(north1, south1, west1, east1, 5000, () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Bounding box 1 progress: { (double)cntInc / expectedReqs1 * 100 }%, Total progress: { (double)cnt / expectedReqs * 100 }%");
                Console.WriteLine($"Request processed ( { ++cntInc }, total count: { ++cnt })");
            }, lostData));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Bounding box 1 processed ( { cntInc }, total count: { cnt })");

            // Second bounding box
            cntInc = 0;
            PlacesData.AddPlacesTo(ret, ScanBoundingBox(north2, south2, west2, east2, 5000, () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Bounding box 2 progress: { (double)cntInc / expectedReqs2 * 100 }%, Total progress: { (double)cnt / expectedReqs * 100 }%");
                Console.WriteLine($"Request processed ( { ++cntInc }, total count: { ++cnt })");
            }, lostData));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Bounding box 2 processed ( { cntInc }, total count: { cnt })");

            // Third bounding box
            cntInc = 0;
            PlacesData.AddPlacesTo(ret, ScanBoundingBox(north3, south3, west3, east3, 5000, () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Bounding box 3 progress: { (double)cntInc / expectedReqs3 * 100 }%, Total progress: { (double)cnt / expectedReqs * 100 }%");
                Console.WriteLine($"Request processed ( { ++cntInc }, total count: { ++cnt })");
            }, lostData));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Bounding box 3 processed ( { cntInc }, total count: { cnt })");

            // Fourth bounding box
            cntInc = 0;
            PlacesData.AddPlacesTo(ret, ScanBoundingBox(north4, south4, west4, east4, 5000, () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Bounding box 4 progress: { (double)cntInc / expectedReqs4 * 100 }%, Total progress: { (double)cnt / expectedReqs * 100 }%");
                Console.WriteLine($"Request processed ( { ++cntInc }, total count: { ++cnt })");
            }, lostData));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Bounding box 4 processed ( { cntInc }, total count: { cnt })");

            // Budapest bounding box
            cntInc = 0;
            PlacesData.AddPlacesTo(ret, ScanBoundingBox(north5, south5, west5, east5, 2000, () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Bounding box 5 progress: { (double)cntInc / expectedReqs5 * 100 }%, Total progress: { (double)cnt / expectedReqs * 100 }%");
                Console.WriteLine($"Request processed ( { ++cntInc }, total count: { ++cnt })");
            }, lostData));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Bounding box 5 processed ( { cntInc }, total count: { cnt })");

            requestCnt = cnt;

            return ret;
        }

        

        /// <summary>
        /// Scans a bounding box with Google Places API for train stations
        /// </summary>
        /// <param name="north">North border Latitude</param>
        /// <param name="south">South border Latitude</param>
        /// <param name="west">West border Longitude</param>
        /// <param name="east">East border Longitude</param>
        /// <param name="radiusM">Scan radius in meters</param>
        /// <param name="requestProcessed">Action to take when a request is processed</param>
        /// <param name="lostData">GPS coordinates Latitude (X), Longitude (Y) where there can be stations not returned by the request (more than 20)</param>
        /// <returns>A list of place names and coordinates for every train station in the area</returns>
        public static List<PlacesData> ScanBoundingBox(double north, double south, double west, double east, double radiusM, Action requestProcessed, List<Vector2> lostData)
        {
            List<PlacesData> ret = new List<PlacesData>();
            Vector2 topLeft = Map.DefaultMap.FromLatLon(new Vector2(north, west));
            Vector2 bottomLeft = Map.DefaultMap.FromLatLon(new Vector2(south, west));
            Vector2 topRight = Map.DefaultMap.FromLatLon(new Vector2(north, east));
            Vector2 bottomRight = Map.DefaultMap.FromLatLon(new Vector2(south, east));

            for (double x = topLeft.X; x < topRight.X; x += 4.0 / 3.0 * radiusM / Map.DefaultMap.MeterPerWebMercUnit())
            {
                for (double y = topLeft.Y; y < bottomLeft.Y; y += 4.0 / 3.0 * radiusM / Map.DefaultMap.MeterPerWebMercUnit())
                {
                    List<PlacesData> places = ScanGPSCoord(new Vector2(x, y), radiusM, lostData);
                    PlacesData.AddPlacesTo(ret, places);
                    requestProcessed();
                }
            }

            return ret;
        }

        /// <summary>
        /// Searches with Google Places API for train stations near a GPS coordinate
        /// </summary>
        /// <param name="gpsCoord">Center of the search as GPS coordinate Latitude (X), Longitude (Y)</param>
        /// <param name="radiusM">Scan radius in meters</param>
        /// <param name="lostData">GPS coordinates Latitude (X), Longitude (Y) where there can be stations not returned by the request (more than 20)</param>
        /// <returns>A list of place names and coordinates for every train station in the area</returns>
        public static List<PlacesData> ScanGPSCoord(Vector2 gpsCoord, double radiusM, List<Vector2> lostData)
        {
            List<PlacesData> places = RequestPlaces(Map.DefaultMap.ToLatLon(gpsCoord), (int)(radiusM));
            // Check for more than 20 possibly lost stations
            if (places.Count >= 20)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!!!SOME STATIONS ARE LOST OVER 20!!! " + places.Count);
                lostData.Add(Map.DefaultMap.ToLatLon(gpsCoord));
            }

            return places;
        }

        /// <summary>
        /// Write a list of PlacesData into a stream
        /// </summary>
        /// <param name="places">List of PlacesData (name, GPS coord)</param>
        /// <param name="stream">Stream to write to</param>
        public static void WritePlacesDataToStream(List<PlacesData> places, Stream stream)
        {
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            foreach (PlacesData place in places)
            {
                writer.WriteLine($"{place.Name}|{place.GPSCoord}");
            }
            writer.Close();
        }

        /// <summary>
        /// Reads a list of PlacesData from a stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns>List of PlacesData (name, GPS coord)</returns>
        public static List<PlacesData> ReadPlacesDataFromStream(Stream stream)
        {
            List<PlacesData> places = new List<PlacesData>();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split('|');
                string[] coordParts = parts[1].Split(',');
                places.Add(new PlacesData(parts[0], new Vector2(coordParts[0], coordParts[1])));
            }
            reader.Close();
            return places;
        }
    }
}
