using System;
using System.Data.Common;
using MAVAppBackend.DataAccess;
using Newtonsoft.Json.Linq;
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

        public TrainStation TrainStation { get; private set; }

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
            TrainStation = Database.Instance.TrainStationMapper.IsBatchBegun ? Database.Instance.TrainStationMapper.GetByKey(trainStationID) : null;
            actualArrival = reader.GetTimeSpanOrNull("actual_arrival");
            actualDeparture = reader.GetTimeSpanOrNull("actual_departure");
            Filled = true;
        }

        public override void Fill(Entity<int> other)
        {
            if (!(other is TrainInstanceStation trainInstanceStation)) return;

            Key = trainInstanceStation.Key;
            trainInstanceID = trainInstanceStation.trainInstanceID;
            trainStationID = trainInstanceStation.trainStationID;
            TrainStation = trainInstanceStation.TrainStation;
            actualArrival = trainInstanceStation.actualArrival;
            actualDeparture = trainInstanceStation.actualDeparture;
            Filled = trainInstanceStation.Filled;
        }

        public JObject ToJObject()
        {
            JObject o = new JObject()
            {
                ["actual-arrival"] = actualArrival?.ToString(@"hh\:mm"),
                ["actual-departure"] = actualDeparture?.ToString(@"hh\:mm")
            };

            if (TrainStation != null)
            {
                o.Merge(TrainStation?.ToJObject());
            }

            return o;
        }
    }
}
