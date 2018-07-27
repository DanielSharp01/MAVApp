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
                Database.Instance.TestMapper.BeginSelect();
                TestEntity david = Database.Instance.TestMapper.GetByKey(1);
                TestEntity ellie = Database.Instance.TestMapper.GetByKey(2);
                Database.Instance.TestMapper.EndSelect();
                david.Money -= 100;
                david.Coord = new Vector2(40, 60);
                ellie.Money += 100;
                david.Name = "<cannot be set>";
                Database.Instance.TestMapper.BeginUpdate();
                Database.Instance.TestMapper.UpdateSaveCache();
                Database.Instance.TestMapper.EndUpdate();
            }

            Console.ReadLine();
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();
    }
}
