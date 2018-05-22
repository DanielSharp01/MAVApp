using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MAVAppBackend
{
    public class Program
    {
        //TODO: Exceptions of the Places API

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

            request.GetRequestStream().Write(payload, 0, payload.Length);
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress), Encoding.UTF8))
                {
                    return JObject.Parse(reader.ReadToEnd());
                }
            }
        }

        static Train GetTrain(string elviraID)
        {
            JObject trainRequest = new JObject();
            trainRequest["a"] = "TRAIN";
            trainRequest["jo"] = new JObject();
            trainRequest["jo"]["v"] = elviraID;
            return new Train(elviraID, RequestMAV(trainRequest));
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Train train = GetTrain(Console.ReadLine());

            Console.WriteLine("Elvira ID: " + train.ElviraID);
            Console.WriteLine("Train number: " + train.Number);
            if (train.Name != null) Console.WriteLine("Train name: " + train.Name);
            Console.WriteLine("Train type: " + train.Type);
            if (train.NumberType != null) Console.WriteLine("Train number type: " + train.Type);
            Console.WriteLine("Elvira ID: " + train.ElviraID);
            Console.WriteLine(train.DelayReason);
            if (train.MiscInfo.Any()) Console.WriteLine("Misc info:");
            foreach (string info in train.MiscInfo)
            {
                Console.WriteLine(info);
            }

            foreach (StationInfo station in train.Stations)
            {
                Console.WriteLine($"{station.Station.Name} - " + (station.IntDistance == -1 ? "distance unknown" : station.IntDistance + "km") + $" - Arrival: {station.Arrival.ToString("yyyy. MM. dd. HH:mm")};" +
                    $"expected: {station.ExpectedArrival.ToString("yyyy. MM. dd. HH:mm")} - Departure: {station.Departure.ToString("yyyy. MM. dd. HH:mm")};" +
                    $"expected: {station.ExpectedDeparture.ToString("yyyy. MM. dd. HH:mm")} - Platform: {station.Platform ?? "unknown"} - " + (station.Arrived ? "arrived" : "not arrived"));
            }

            SVGStream svg = new SVGStream(@"C:\Users\DanielSharp\Desktop\asd.svg", 1920, 1080);
            train.PrintIntoSVG(svg);
            svg.Close();
        }
    }
}
