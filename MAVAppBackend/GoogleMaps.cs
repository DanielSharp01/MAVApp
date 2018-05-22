using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Name with Latitude, longitude information.
    /// See also: <seealso cref="GoogleMaps.RequestPlaces"/>
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
    }

    class PlaceAPIException : Exception
    {
        public PlaceAPIException(string message) : base(message)
        { }
    }


    /// <summary>
    /// Google maps APIs
    /// </summary>
    public class GoogleMaps
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
    }
}
