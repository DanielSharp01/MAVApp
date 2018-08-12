using System;
using System.Linq;
using MAVAppBackend.APIHandlers;
using MAVAppBackend.DataAccess;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Database.Instance)
            {
                while (HandleInput(Console.ReadLine())) ;
            }
        }

        public static bool HandleInput(string input)
        {
            if (input == "quit" || input == "exit") return false;

            if (input.StartsWith("route"))
            {
                input = input.Substring("route".Length);
                string[] parameters = input.Split(",").Select(s => s.Trim()).ToArray();
                if (parameters.Length == 3)
                {
                    MAVAPI.RequestRoute(parameters[0], parameters[1], parameters[2], DateTime.Now).UpdateDatabase();
                }
                else if (parameters.Length == 2)
                {
                    MAVAPI.RequestRoute(parameters[0], parameters[1], null, DateTime.Now).UpdateDatabase();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ROUTE needs at least 2 parameters");
                    Console.ResetColor();
                }
            }
            else if (input == "trains")
            {
                MAVAPI.RequestTrains().UpdateDatabase();;
            }
            else if (input.StartsWith("train"))
            {
                input = input.Substring("train".Length);
                if (input.Contains('_'))
                {
                    MAVAPI.RequestTrain(input).UpdateDatabase();
                }
                else
                {
                    MAVAPI.RequestTrain(int.Parse(input)).UpdateDatabase();
                }
            }
            else if (input.StartsWith("station"))
            {
                input = input.Substring("station".Length);
                MAVAPI.RequestStation(input.Trim(), DateTime.Now).UpdateDatabase();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"'{input}' is not an existing command.");
                Console.ResetColor();
            }

            return true;
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}

