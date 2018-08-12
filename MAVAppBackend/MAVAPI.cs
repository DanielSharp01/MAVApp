using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using MAVAppBackend.APIHandlers;

namespace MAVAppBackend
{
    /// <summary>
    /// Exception thrown if the MAV API fails
    /// </summary>
    public class MAVAPIException : Exception
    {
        public MAVAPIException(string message)
            : base(message)
        { }

        public MAVAPIException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <inheritdoc />
    /// <summary>
    /// Exception thrown if the parsing of MAV API returned HTMLs fail
    /// </summary>
    public class MAVParseException : MAVAPIException
    {
        public MAVParseException(string message)
            : base(message)
        { }

        public MAVParseException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Contains API calls for the MAV "APIs"
    /// </summary>
    public class MAVAPI
    {
        /// <summary>
        /// Requests MAV with a JSON represented by a JObject
        /// </summary>
        /// <param name="requestObject">Object with requested data</param>
        /// <returns>A JObject of the response</returns>
        public static JObject RequestMAV(JObject requestObject)
        {
            HttpWebRequest request = WebRequest.CreateHttp("http://vonatinfo.mav-start.hu/map.aspx/getData");
            byte[] payload = Encoding.UTF8.GetBytes(requestObject.ToString(Formatting.None));

            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers["Accept-Encoding"] = "gzip, deflate";
            request.Headers["Accept-Language"] = "hu-HU,hu;q=0.9,en-US;q=0.8,en;q=0.7";
            request.ContentLength = payload.Length;
            request.ContentType = "application/json; charset=UTF-8";
            request.Host = "vonatinfo.mav-start.hu";
            request.Headers["Origin"] = "http://vonatinfo.mav-start.hu/";
            request.Referer = "http://vonatinfo.mav-start.hu/";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.Method = "POST";

            try
            {
                request.GetRequestStream().Write(payload, 0, payload.Length);
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress), Encoding.UTF8))
                    {
                        return JObject.Parse(reader.ReadToEnd());
                    }
                }
            }
            catch (WebException e)
            {
                throw new MAVAPIException("Could not resolve JSON request.", e);
            }
        }

        /// <summary>
        /// Request the TRAIN API
        /// </summary>
        /// <param name="elviraID">Unique Key per train class</param>
        /// <returns>An API handler object</returns>
        public static TRAINHandler RequestTrain(int trainId)
        {
            JObject requestParameters = new JObject
            {
                ["a"] = "TRAIN",
                ["jo"] = new JObject {["vsz"] = "55" + trainId}
            };
            return new TRAINHandler(RequestMAV(requestParameters));
        }

        /// <summary>
        /// Request the TRAIN API
        /// </summary>
        /// <param name="elviraID">Unique Key</param>
        /// <returns>An API handler object</returns>
        public static TRAINHandler RequestTrain(string elviraID)
        {
            JObject requestParameters = new JObject
            {
                ["a"] = "TRAIN",
                ["jo"] = new JObject {["v"] = elviraID}
            };
            return new TRAINHandler(RequestMAV(requestParameters));
        }

        /// <summary>
        /// Request the STATION API
        /// </summary>
        /// <param name="stationName">Name of the station</param>
        /// <param name="date">Date of the timetable</param>
        /// <returns>An API handler object</returns>
        public static STATIONHandler RequestStation(string stationName, DateTime date)
        {
            JObject requestParameters = new JObject
            {
                ["a"] = "STATION",
                ["jo"] = new JObject
                {
                    ["a"] = stationName,
                    ["d"] = date.ToString("yyyy.MM.dd")
                }
            };
            return new STATIONHandler(RequestMAV(requestParameters));
        }

        /// <summary>
        /// Request the ROUTE API
        /// </summary>
        /// <param name="from">Name of the station to plan from</param>
        /// <param name="to">Name of the station to plan to</param>
        /// <param name="touching">Name of the station to plan touching</param>
        /// <param name="date">Date of the timetable</param>
        /// <returns>An API handler object</returns>
        public static ROUTEHandler RequestRoute(string from, string to, string touching, DateTime date)
        {
            JObject requestParameters = new JObject
            {
                ["a"] = "ROUTE",
                ["jo"] = new JObject
                {
                    ["i"] = @from,
                    ["e"] = to
                }
            };
            if (touching != null) requestParameters["jo"]["v"] = touching;
            return new ROUTEHandler(RequestMAV(requestParameters));
        }

        /// <summary>
        /// Request TRAINS API for data about trains (position, delay) from MÁV
        /// </summary>
        /// <returns></returns>
        public static TRAINSHandler RequestTrains()
        {
            JObject requestParameters = new JObject
            {
                ["a"] = "TRAINS",
                ["jo"] = new JObject
                {
                    ["history"] = false,
                    ["id"] = false
                }
            };
            return new TRAINSHandler(RequestMAV(requestParameters));
        }
    }
}
