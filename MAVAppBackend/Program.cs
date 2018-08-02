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
                Database.Instance.StationMapper.BeginUpdate();
                Database.Instance.StationMapper.Update(new Station(-1) {Name = "test", NormalizedName = "test"});
                Database.Instance.StationMapper.EndUpdate();
            }

            Console.ReadLine();
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}
