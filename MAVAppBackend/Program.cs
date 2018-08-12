using System;
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
                JObject obj = MAVAPI.RequestTrain("104904_180812");
                new TRAINHandler(obj).UpdateDatabase();

                obj = MAVAPI.RequestRoute("Budapest-Nyugati", "Monor", null, DateTime.Now);
                new ROUTEHandler(obj).UpdateDatabase();

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

