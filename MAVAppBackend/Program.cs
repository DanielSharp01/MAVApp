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
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            string elviraID = Console.ReadLine();
            Train train = MAVAPI.GetTrain(elviraID);
            if (train == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Could not resolve train with ID {elviraID}");
                Console.ResetColor();
            }
            else
            {
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

            Console.ReadLine();
        }
    }
}
