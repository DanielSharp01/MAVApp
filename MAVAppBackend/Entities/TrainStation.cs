using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class TrainStation : UpdatableEntity<int>
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

        private int ordinal;
        public int Ordinal
        {
            get => ordinal;
            set
            {
                ordinal = value;
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

        private DateTime? arrival;
        public DateTime? Arrival
        {
            get => arrival;
            set
            {
                arrival = value;
                OnChange();
            }
        }

        private DateTime? departure;
        public DateTime? Departure
        {
            get => departure;
            set
            {
                departure = value;
                OnChange();
            }
        }

        private double relativeDistance;
        public double RelativeDistance
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

        public TrainStation(int key)
            : base(key)
        { }

        protected override void InternalFill(DbDataReader reader)
        {
            trainID = reader.GetInt32("train_id");
            ordinal = reader.GetInt32("ordinal");
            stationID = reader.GetInt32("station_id");
            arrival = reader.GetDateTimeOrNull("arrival");
            departure = reader.GetDateTimeOrNull("departure");
            relativeDistance = reader.GetDouble("rel_distance");
            platform = reader.GetStringOrNull("platform");
            Filled = true;
        }

        public override void Fill(UpdatableEntity<int> other)
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
