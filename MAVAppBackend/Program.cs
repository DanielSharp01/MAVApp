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
            //StreamWriter writer = new StreamWriter(@"C:\users\DanielSharp\Desktop\train_api.html");
            //JObject jobj = MAVAPI.RequestTrain(521);
            //writer.Write(jobj["d"]["result"]["html"]);
            //writer.Close();

            //writer = new StreamWriter(@"C:\users\DanielSharp\Desktop\station_api.html");
            //jobj = MAVAPI.RequestStation("Budapest-Nyugati", DateTime.Now);
            //writer.Write(jobj["d"]["result"]);
            //writer.Close();

            //writer = new StreamWriter(@"C:\users\DanielSharp\Desktop\route_api.html");
            //jobj = MAVAPI.RequestRoute("Budapest-Nyugati", "Cegléd", null, DateTime.Now);
            //writer.Write(jobj["d"]["result"]);
            //writer.Close();
            using (Database db = Database.Instance)
            {
                BuildWebHost(args).Run();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
