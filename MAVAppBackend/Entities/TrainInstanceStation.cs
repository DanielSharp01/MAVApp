using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class TrainInstanceStation : Entity<int>
    {
        private int trainInstanceID;
        public int TrainInstanceID
        {
            get => trainInstanceID;
            set
            {
                trainInstanceID = value;
                OnChange();
            }
        }

        private int trainStationID;
        public int TrainStationID
        {
            get => trainStationID;
            set
            {
                trainStationID = value;
                OnChange();
            }
        }

        private DateTime? actualArrival;
        public DateTime? ActualArrival
        {
            get => actualArrival;
            set
            {
                actualArrival = value;
                OnChange();
            }
        }

        private DateTime? actualDeparture;
        public DateTime? ActualDeparture
        {
            get => actualDeparture;
            set
            {
                actualDeparture = value;
                OnChange();
            }
        }
        public override void Fill(DbDataReader reader)
        {
            trainInstanceID = reader.GetInt32("train_instance_id");
            trainStationID = reader.GetInt32("train_station_id");
            actualArrival = reader.GetDateTimeOrNull("actual_arrival");
            actualDeparture = reader.GetDateTimeOrNull("actual_departure");
            Filled = true;
        }

        public override void Fill(Entity<int> other)
        {
            if (!(other is TrainInstanceStation trainStation)) return;

            Key = trainStation.Key;
            trainInstanceID = trainStation.trainInstanceID;
            trainStationID = trainStation.trainStationID;
            actualArrival = trainStation.actualArrival;
            actualDeparture = trainStation.actualDeparture;
            Filled = trainStation.Filled;
        }
    }
}
