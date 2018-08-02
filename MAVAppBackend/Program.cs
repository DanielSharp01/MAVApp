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
                Database.Instance.TrainMapper.Update(new Train(1) {ExpiryDate = DateTime.Now.AddDays(20), Name="Z50", Polyline = null, Type="zónázó"});
                Database.Instance.TrainInstanceMapper.Update(new TrainInstance(154879180208) {TrainID = 1 });
            }

            Console.ReadLine();
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}

