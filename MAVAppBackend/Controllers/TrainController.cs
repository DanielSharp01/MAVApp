using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;

namespace MAVAppBackend.Controllers
{
    [Route("train")]
    public class TrainController : APIController
    {
        [HttpGet]
        public IActionResult Get(string[] ids, [ModelBinder(Name = "include-stations")] bool includeStations)
        {
            List<TrainInstance> trainInstances = new List<TrainInstance>();
            if (includeStations)
            {
                Database.Instance.StationMapper.BeginSelect();
                Database.Instance.TrainStationMapper.BeginSelect();
                Database.Instance.TrainInstanceStationMapper.ByTrainInstanceID.BeginSelect();
            }
            Database.Instance.TraceMapper.ByTrainInstanceID.BeginSelect();
            Database.Instance.TrainMapper.BeginSelect();

            foreach (var id in ids)
            {
                if (!TrainInstance.TryGetInstanceID(id, out long lid))
                {
                    trainInstances.Add(new TrainInstance() { Filled = false });
                }
                else
                {
                    trainInstances.Add(Database.Instance.TrainInstanceMapper.GetByKey(lid));
                }

            }

            Database.Instance.TrainMapper.EndSelect();
            Database.Instance.TraceMapper.ByTrainInstanceID.EndSelect();
            if (includeStations)
            {
                Database.Instance.TrainInstanceStationMapper.ByTrainInstanceID.EndSelect();
                Database.Instance.TrainStationMapper.EndSelect();
                Database.Instance.StationMapper.EndSelect();
            }

            JObject dictionary = new JObject();
            for (int i = 0; i < ids.Length; i++)
            {
                dictionary[ids[i]] = trainInstances[i].Filled ? trainInstances[i].ToJObject() : null;
            }

            return Success(dictionary);
        }
    }
}
