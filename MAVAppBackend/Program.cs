using System;
using System.Linq;
using MAVAppBackend.APIHandlers;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Database.Instance)
            {
                //JObject obj = MAVAPI.RequestTrain("105901_180811");
                //new TRAINHandler(obj).UpdateDatabase();

                JObject obj = MAVAPI.RequestStation("Budapest-Nyugati", DateTime.Now);
                new STATIONHandler(obj).UpdateDatabase();

                //JObject obj = MAVAPI.RequestTrains();
                //new TRAINSHandler(obj).UpdateDatabase();

                //Console.WriteLine(Database.Instance.TraceMapper.ByTrainInstanceID.GetByKey(105397180811).First().GPSCoord);
            }

            Console.ReadLine();
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}

