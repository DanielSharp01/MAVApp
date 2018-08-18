using System;
using System.Data.Common;
using System.Linq;
using MAVAppBackend.DataAccess;
using Newtonsoft.Json.Linq;
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

        public Train Train { get; private set; }

        public ListEntityCollection<long, int, TrainInstanceStation> Stations { get; private set; }

        public ListEntityCollection<long, int, Trace> Trace { get; private set; }

        public override void Fill(DbDataReader reader)
        {
            Key = reader.GetInt64("id");
            ElviraID = GetElviraID(Key);
            Date = GetDateTime(Key);
            trainID = reader.GetInt32OrNull("train_id");
            Train = (Database.Instance.TrainMapper.IsBatchBegun && TrainID.HasValue) ? Database.Instance.TrainMapper.GetByKey(trainID.Value) : null;
            Stations = Database.Instance.TrainInstanceStationMapper.ByTrainInstanceID.IsBatchBegun ? Database.Instance.TrainInstanceStationMapper.ByTrainInstanceID.GetByKey(Key) : null;
            Trace = Database.Instance.TraceMapper.ByTrainInstanceID.IsBatchBegun ? Database.Instance.TraceMapper.ByTrainInstanceID.GetByKey(Key) : null;
            Filled = true;
        }

        public override void Fill(Entity<long> other)
        {
            if (!(other is TrainInstance trainInstance)) return;

            Key = trainInstance.Key;
            Date = trainInstance.Date;
            trainID = trainInstance.trainID;
            Train = trainInstance.Train;
            foreach (var trainStation in trainInstance.Stations)
            {
                Stations.Add(trainStation);
            }
            Filled = trainInstance.Filled;
        }

        public static long GetInstanceID(string elviraID)
        {
            return long.Parse(elviraID.Remove(elviraID.IndexOf('_'), 1));
        }

        public static bool TryGetInstanceID(string elviraID, out long id)
        {
            int sep = elviraID.IndexOf('_');
            if (sep == -1) { id = -1; return false; }
            return long.TryParse(elviraID.Remove(sep, 1), out id);
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

        public JObject ToJObject()
        {
            JObject o = Train?.ToJObject() ?? new JObject() { ["elvira-id"] = ElviraID };

            if (Stations != null)
            {
                JArray array = new JArray();
                foreach (var station in Stations)
                {
                    array.Add(station.ToJObject());
                }
                o["stations"] = array;
            }

            Trace trace = Trace.FirstOrDefault();

            if (trace != null && (DateTime.UtcNow - trace.Updated).TotalSeconds <= 20)
                o["gps-coord"] = trace.GPSCoord?.ToJObject();

            return o;
        }
    }
}
