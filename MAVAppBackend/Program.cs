using System;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;

namespace MAVAppBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Database.Instance)
            {
                Database.Instance.TrainStationMapper.ByTrainID.BeginSelect();
                var coll1 = Database.Instance.TrainStationMapper.ByTrainID.GetByKey(1);
                var coll2 = Database.Instance.TrainStationMapper.ByTrainID.GetByKey(2);
                Database.Instance.TrainStationMapper.ByTrainID.EndSelect();
                foreach (var TrainStation in coll1)
                {
                    Console.WriteLine(Database.Instance.StationMapper.GetByKey(TrainStation.StationID).Name);
                }
                foreach (var TrainStation in coll2)
                {
                    Console.WriteLine(Database.Instance.StationMapper.GetByKey(TrainStation.StationID).Name);
                }
            }

            Console.ReadLine();
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}

