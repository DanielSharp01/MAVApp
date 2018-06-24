using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.Controller
{
    [Route("route-trains")]
    public class RouteTrainsController : APIController
    {
        [HttpGet]
        public IActionResult Get([ModelBinder(Name = "from-station")] string fromStation = null, [ModelBinder(Name = "to-station")] string toStation = null,
            [ModelBinder(Name = "from-time")] string fromTime = null, [ModelBinder(Name = "to-time")] string toTime = null)
        {   
            if (fromStation == null || toStation == null || fromTime == null || toTime == null)
                return BadRequestError("Parameters from-station, to-station, from-time and to-time must exist.");

            return StatusCodeWithJObject(501, (new JObject() { ["from-station"] = fromStation, ["to-station"] = toStation, ["from-time"] = fromTime, ["to-time"] = toTime}), "parameter-echo");
        }
    }
}
