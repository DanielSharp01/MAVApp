using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MAVAppBackend
{
    [Route("dbtrain")]
    public class DBTrainController : APIController
    {
        // GET: api/<controller>
        [HttpGet("{id:int}")]
        public IActionResult Get(int id, bool update = false, string data = "both")
        {
            switch (data)
            {
                case "static only": return Success(Train(Database.GetTrain(id, update), TrainDataFilter.StaticOnly));
                case "dynamic only": return Success(Train(Database.GetTrain(id, update), TrainDataFilter.DynamicOnly));
                case "both": return Success(Train(Database.GetTrain(id, update), TrainDataFilter.Both));
                default: return BadRequestError("Parameter 'data' must be either 'static only', 'dynamic only' or 'both'.");
            }
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(int[] filter, string data = "both")
        {
            if (filter.Length == 0)
                return BadRequestError("Parameter array 'filter' has no valid (integer) values in it.");

            TrainDataFilter dataFilter;
            switch (data)
            {
                case "static only": dataFilter = TrainDataFilter.StaticOnly; break;
                case "dynamic only": dataFilter = TrainDataFilter.DynamicOnly; break;
                case "both": dataFilter = TrainDataFilter.Both; break;
                default: return BadRequestError("Parameter 'data' must be either 'static only', 'dynamic only' or 'both'.");
            }

            JObject response = new JObject();
            foreach (int id in filter)
            {
                response[id.ToString()] = Train(Database.GetTrain(id, false), dataFilter);
            }

            return Success(response);
        }
    }
}
