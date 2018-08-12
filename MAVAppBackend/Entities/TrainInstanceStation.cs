using System;
using System.Data.Common;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class TrainInstanceStation : Entity<int>
    {
        private long trainInstanceID;
        public long TrainInstanceID
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

        private TimeSpan? actualArrival;
        public TimeSpan? ActualArrival
        {
            get => actualArrival;
            set
            {
                actualArrival = value;
                OnChange();
            }
        }

        private TimeSpan? actualDeparture;
        public TimeSpan? ActualDeparture
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
            Key = reader.GetInt32("id");
            trainInstanceID = reader.GetInt64("train_instance_id");
            trainStationID = reader.GetInt32("train_station_id");
            actualArrival = reader.GetTimeSpanOrNull("actual_arrival");
            actualDeparture = reader.GetTimeSpanOrNull("actual_departure");
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
