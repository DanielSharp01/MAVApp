using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.Controllers
{
    [Route("station")]
    public class StationController : APIController
    {
        [HttpGet]
        public IActionResult Get(string[] ids)
        {
            List<Station> stations = new List<Station>();
            Database.Instance.StationMapper.BeginSelect();
            Database.Instance.StationMapper.ByNormName.BeginSelect();

            foreach (var id in ids)
            {
                if (int.TryParse(id, out int iid))
                {
                    stations.Add(Database.Instance.StationMapper.GetByKey(iid));
                }
                else
                {
                    stations.Add(Database.Instance.StationMapper.ByNormName.GetByKey(Database.StationNormalizeName(id)));
                }
            }

            Database.Instance.StationMapper.EndSelect();
            Database.Instance.StationMapper.ByNormName.EndSelect();

            JObject dictionary = new JObject();
            for (int i = 0; i < ids.Length; i++)
            {
                dictionary[ids[i]] = stations[i].Filled ? stations[i].ToJObject() : null;
            }

            return Success(dictionary);
        }
    }
}
