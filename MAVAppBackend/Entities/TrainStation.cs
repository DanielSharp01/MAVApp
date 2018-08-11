using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class TrainStation : OrdinalEntity<int>
    {
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

        private int stationID;
        public int StationID
        {
            get => stationID;
            set
            {
                stationID = value;
                OnChange();
            }
        }

        private TimeSpan? arrival;
        public TimeSpan? Arrival
        {
            get => arrival;
            set
            {
                arrival = value;
                OnChange();
            }
        }

        private TimeSpan? departure;
        public TimeSpan? Departure
        {
            get => departure;
            set
            {
                departure = value;
                OnChange();
            }
        }

        private double? relativeDistance;
        public double? RelativeDistance
        {
            get => relativeDistance;
            set
            {
                relativeDistance = value;
                OnChange();
            }
        }

        private string platform;
        public string Platform
        {
            get => platform;
            set
            {
                platform = value;
                OnChange();
            }
        }
        public override void Fill(DbDataReader reader)
        {
            Key = reader.GetInt32("id");
            trainID = reader.GetInt32("train_id");
            ordinal = reader.GetInt32("ordinal");
            stationID = reader.GetInt32("station_id");
            arrival = reader.GetTimeSpanOrNull("arrival");
            departure = reader.GetTimeSpanOrNull("departure");
            relativeDistance = reader.GetDoubleOrNull("rel_distance");
            platform = reader.GetStringOrNull("platform");
            Filled = true;
        }

        public override void Fill(Entity<int> other)
        {
            if (!(other is TrainStation trainStation)) return;

            Key = trainStation.Key;
            trainID = trainStation.trainID;
            ordinal = trainStation.ordinal;
            stationID = trainStation.stationID;
            arrival = trainStation.arrival;
            departure = trainStation.departure;
            relativeDistance = trainStation.relativeDistance;
            platform = trainStation.platform;
            Filled = trainStation.Filled;
        }
    }
}
