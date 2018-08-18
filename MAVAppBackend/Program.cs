using System;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MAVAppBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // /train?ids=105230_180818&include-stations=true
            using (Database.Instance)
            {
                Task.Run(() =>
                {
                    while (HandleInput(Console.ReadLine()))
                    { }
                });
                BuildWebHost(args).Run();
            }
        }

        public static bool HandleInput(string input)
        {
            if (input == "quit" || input == "exit" || input == null) return false;

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

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}

