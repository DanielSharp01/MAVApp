using System.Collections.Generic;
using System.Linq;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.Controllers
{
    public class APIController : ControllerBase
    {
        public IActionResult Success(JToken obj)
        {
            return StatusCodeWithJObject(200, obj, "result");
        }

        public IActionResult BadRequestError(JToken obj)
        {
            return StatusCodeWithJObject(400, obj, "error");
        }

        public IActionResult StatusCodeWithJObject(int statusCode, JToken obj, string resultFieldName = "result")
        {
            JObject response = new JObject
            {
                ["status"] = statusCode,
                [resultFieldName] = obj
            };
            return StatusCode(500, response.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
