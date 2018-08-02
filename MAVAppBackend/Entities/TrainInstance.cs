using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class TrainInstance : UpdatableEntity<long>
    {
        public string ElviraID { get; private set; }
        public DateTime Date { get; private set; }

        private int trainID;
        public int TrainID
        {
            get => trainID;
            set
            {
                trainID = value;
                OnChange();
            }
        }

        public TrainInstance(long key)
            : base(key)
        { }

        protected override void InternalFill(DbDataReader reader)
        {
            long id = Key;
            long date = id % 10000000000;
            id /= 100;
            long month = id % 100000000;
            id /= 100;
            long year = id % 1000000;

            ElviraID = (id / 1000000) + "_" + (id % 1000000);
            Date = new DateTime(DateTime.Now.Year - DateTime.Now.Year % 100 + (int)year, (int)month, (int)date);
            trainID = reader.GetInt32("train_id");
            Filled = true;
        }

        public override void Fill(UpdatableEntity<long> other)
        {
            if (!(other is TrainInstance trainInstance)) return;

            Key = trainInstance.Key;
            Date = trainInstance.Date;
            trainID = trainInstance.trainID;
            Filled = trainInstance.Filled;
        }
    }
}
