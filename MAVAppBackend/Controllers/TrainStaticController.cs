using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;

namespace MAVAppBackend.Controllers
{
    [Route("train-static")]
    public class TrainStaticController : APIController
    {
        [HttpGet]
        public IActionResult Get(int[] ids, [ModelBinder(Name = "include-stations")] bool includeStations)
        {
            List<Train> trains = new List<Train>();
            if (includeStations)
            {
                Database.Instance.StationMapper.BeginSelect();
                Database.Instance.TrainStationMapper.ByTrainID.BeginSelect();
            }
            Database.Instance.TrainMapper.BeginSelect();

            foreach (var id in ids)
            {
                trains.Add(Database.Instance.TrainMapper.GetByKey(id));
            }

            Database.Instance.TrainMapper.EndSelect();
            if (includeStations)
            {
                Database.Instance.TrainStationMapper.ByTrainID.EndSelect();
                Database.Instance.StationMapper.EndSelect();
            }

            JObject dictionary = new JObject();
            for (int i = 0; i < ids.Length; i++)
            {
                dictionary[ids[i].ToString()] = trains[i].Filled ? trains[i].ToJObject() : null;
            }

            return Success(dictionary);
        }
    }
}
