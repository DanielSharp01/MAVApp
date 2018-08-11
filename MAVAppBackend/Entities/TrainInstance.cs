using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using Microsoft.CodeAnalysis.Semantics;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class TrainInstance : Entity<long>
    {
        public string ElviraID { get; private set; }
        public DateTime Date { get; private set; }

        private int? trainID;
        public int? TrainID
        {
            get => trainID;
            set
            {
                trainID = value;
                OnChange();
            }
        }

        public override void Fill(DbDataReader reader)
        {
            ElviraID = GetElviraID(Key);
            Date = GetDateTime(Key);
            trainID = reader.GetInt32OrNull("train_id");
            Filled = true;
        }

        public override void Fill(Entity<long> other)
        {
            if (!(other is TrainInstance trainInstance)) return;

            Key = trainInstance.Key;
            Date = trainInstance.Date;
            trainID = trainInstance.trainID;
            Filled = trainInstance.Filled;
        }

        public static long GetInstanceID(string elviraID)
        {
            return long.Parse(elviraID.Remove(elviraID.IndexOf('_'), 1));
        }

        public static DateTime GetDateTime(long instanceID)
        {
            long datePart = instanceID % 1000000;
            long date = datePart % 100;
            datePart /= 100;
            long month = datePart % 100;
            datePart /= 100;
            long year = datePart;
            
            return new DateTime(DateTime.Now.Year - DateTime.Now.Year % 100 + (int)year, (int)month, (int)date);
        }

        public static string GetElviraID(long instanceID)
        {
            return (instanceID / 1000000) + "_" + (instanceID % 1000000);
        }
    }
}
