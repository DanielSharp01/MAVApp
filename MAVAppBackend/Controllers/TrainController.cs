using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    [Route("train/{id}")]
    public class TrainController : APIController
    {
        [HttpGet]
        public IActionResult Get(string id, bool update = false, string data = "both")
        {
            if (!TryParseTrainDataFilter(data, out TrainDataFilter dataFilter))
                return BadRequestError("Parameter 'data' must be either 'static only', 'dynamic only' or 'both'.");

            Train train;
            if (int.TryParse(id, out int mysqlID))
                train = Database.GetTrain(mysqlID, update);
            else
                train = Database.GetTrainByElviraID(id, update);

            return Success(Train(train, dataFilter));
        }
    }
}
