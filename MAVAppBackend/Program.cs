using System;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;

namespace MAVAppBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Database.Instance)
            {
                Console.WriteLine(((Station)Database.Instance.StationNNKeyMapper.GetByKey("TEST STATION")).Key);
            }

            Console.ReadLine();
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}
