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
                Console.WriteLine(Database.Instance.StationMapper.ByNormName.GetByKey("cegléd").Name);
            }

            Console.ReadLine();
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}

