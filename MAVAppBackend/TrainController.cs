using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MAVAppBackend
{
    [Route("[controller]")]
    public class TrainController : ControllerBase
    {
        // GET: api/<controller>
        [HttpGet("{id}")]
        public IActionResult Get(int id, bool update)
        {
            return Ok(Database.GetTrain(id, update));
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get([FromQuery(Name="elvira-id")] string elviraID, [FromQuery] bool update = true)
        { 
            if (elviraID == null)
                return BadRequest();
            else return Ok(Database.GetTrainByElviraID(elviraID, update));
        }
    }
}
