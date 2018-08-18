using System.Collections.Generic;
using System.Linq;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.APIHandlers
{
    public class TRAINSHandler
    {
        private JArray trainArray;
        public TRAINSHandler(JObject apiResponse)
        {
            if (apiResponse == null) return;

            trainArray = apiResponse["d"]["result"]["Trains"]["Train"] as JArray;
        }

        public void UpdateDatabase()
        {
            List<TrainInstance> trainInstances = new List<TrainInstance>();
            Database.Instance.TraceMapper.BeginUpdate();
            Database.Instance.TrainInstanceMapper.BeginSelect();
            foreach (var train in trainArray)
            {
                Trace trace = new Trace()
                {
                    Key = -1,
                    TrainInstanceID = TrainInstance.GetInstanceID(train["@ElviraID"].ToString()),
                    GPSCoord = new Vector2(train["@Lat"].ToString(), train["@Lon"].ToString())
                };

                Database.Instance.TraceMapper.Update(trace);
                trainInstances.Add(Database.Instance.TrainInstanceMapper.GetByKey(trace.TrainInstanceID));
            }
            Database.Instance.TrainInstanceMapper.EndSelect();
            Database.Instance.TrainInstanceMapper.Update(trainInstances.GroupBy(i => i.Key).Select(i => i.First()).Where(i => !i.Filled).ToList());
            Database.Instance.TraceMapper.EndUpdate();
        }
    }
}
