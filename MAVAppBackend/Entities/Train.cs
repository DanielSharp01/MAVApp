using System;
using System.Data.Common;
using MAVAppBackend.DataAccess;
using MAVAppBackend.EntityMappers;
using Newtonsoft.Json.Linq;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public class Train : Entity<int>
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnChange();
            }
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                type = value;
                OnChange();
            }
        }

        private Polyline polyline;
        public Polyline Polyline
        {
            get => polyline;
            set
            {
                polyline = value;
                OnChange();
            }
        }

        private DateTime? expiryDate;
        public DateTime? ExpiryDate
        {
            get => expiryDate;
            set
            {
                expiryDate = value;
                OnChange();
            }
        }

        public OrdinalEntityCollection<int, int, TrainStation> Stations { get; private set; }

        public override void Fill(DbDataReader reader)
        {
            name = reader.GetStringOrNull("name");
            type = reader.GetStringOrNull("type");
            polyline = reader.GetPolylineOrNull("polyline");
            expiryDate = reader.GetDateTimeOrNull("expiry_date");
            Stations = Database.Instance.TrainStationMapper.ByTrainID.IsBatchBegun ? Database.Instance.TrainStationMapper.ByTrainID.GetByKey(Key) : null;
            Filled = true;
        }

        public override void Fill(Entity<int> other)
        {
            if (!(other is Train train)) return;

            name = train.name;
            type = train.type;
            polyline = train.polyline;
            expiryDate = train.expiryDate;
            foreach (var trainStation in train.Stations)
            {
                Stations.Add(trainStation);
            }
            Filled = train.Filled;
        }

        public JObject ToJObject()
        {
            JObject o = new JObject()
            {
                ["number"] = Key,
                ["name"] = name,
                ["type"] = type,
                ["encoded-polyline"] = polyline != null ? Polyline.EncodePoints(polyline.Points, 1e5) : null
            };

            if (Stations != null)
            {
                JArray array = new JArray();
                foreach (var station in Stations)
                {
                    array.Add(station.ToJObject());
                }
                o["stations"] = array;
            }

            return o;
        }
    }
}
