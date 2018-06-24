using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using MAVAppBackend.DataAccess;

namespace MAVAppBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //MAVAPI.RequestStation("Budapest-Nyugati", DateTime.Now);
            using (Database db = Database.Instance)
            {
                // DatabaseLegacy.Initialize();
                // DatabaseLegacy.UpdateDynamicData(MAVAPI.RequestTrains());
                BuildWebHost(args).Run();
                // DatabaseLegacy.Terminate();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
