using MAVAppBackend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;

namespace MAVAppBackend.Controller
{
    [Route("station-trains")]
    public class StationTrainsController : APIController
    {
        [HttpGet]
        public IActionResult Get(string station = null, [ModelBinder(Name = "min-time")] string minRange = null, [ModelBinder(Name = "max-time")] string maxRange = null)
        {
            if (station == null)
                return BadRequestError("Parameters station, from-time and to-time must exist.");

            Station stationEntity = Database.Instance.StationMapper.ByNormName.GetByKey(Database.StationNormalizeName(station));
            TimeSpan? minTime = Parsing.TimeFromHoursMinutes(minRange);
            TimeSpan? maxTime = Parsing.TimeFromHoursMinutes(maxRange);

            if (!stationEntity.Filled)
                return BadRequestError("Station does not exist.");

            if (minRange == null || maxRange == null)
                return BadRequestError("Parameters min-time and max-time must exist and be in the format of hh:mm.");

            return StatusCodeWithJObject(501, new JObject()
                {
                    ["station"] = station,
                    ["min-time"] = minTime.Value.ToString("hh:mm"),
                    ["max-time"] = maxTime.Value.ToString("hh:mm")
                }, "parameter-echo");
        }
    }
}
