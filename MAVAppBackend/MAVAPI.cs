using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Exception thrown if the parsing of MAV API returned HTMLs fail
    /// </summary>
    public class MAVAPIException : Exception
    {
        public MAVAPIException(string message) : base(message)
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                return null;
            }
        }

        public static Train GetTrain(string elviraID)
        {
            try
            {
                JObject trainRequest = new JObject();
                trainRequest["a"] = "TRAIN";
                trainRequest["jo"] = new JObject();
                trainRequest["jo"]["v"] = elviraID;
                JObject trainResponse = RequestMAV(trainRequest);
                if (trainResponse == null) return null;
                return new Train(elviraID, trainResponse);
            }
            catch (MAVAPIException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                return null;
            }
        }
    }
}
