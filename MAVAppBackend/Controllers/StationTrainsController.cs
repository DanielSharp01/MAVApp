using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.Controller
{
    [Route("station-trains")]
    public class StationTrainsController : APIController
    {
        [HttpGet]
        public IActionResult Get(string station = null, [ModelBinder(Name = "from-time")] string fromTime = null, [ModelBinder(Name = "to-time")] string toTime = null)
        {   
            if (station == null || fromTime == null || toTime == null)
                return BadRequestError("Parameters station, from-time and to-time must exist.");

            return StatusCodeWithJObject(501, new JObject() { ["station"] = station, ["from-time"] = fromTime, ["to-time"] = toTime}, "parameter-echo");
        }
    }
}
