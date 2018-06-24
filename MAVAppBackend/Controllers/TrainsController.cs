using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MAVAppBackend.Controller
{
    [Route("trains")]
    public class TrainsController : APIController
    {
        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(string[] ids, string data = "both", bool? moving = null, double lat = double.NaN, double lon = double.NaN, double radius = double.NaN)
        {
            if (!TryParseTrainDataFilter(data, out TrainDataFilter dataFilter))
                return BadRequestError("Parameter 'data' must be either 'static only', 'dynamic only' or 'both'.");

            if ((ids.Length > 0 && (!double.IsNaN(lat) || !double.IsNaN(lon) || !double.IsNaN(radius))))
            {
                return BadRequestError("Parameters ids, lat, lon and radius cannot be active at the same time.");
            }

            if ((moving != null && (!double.IsNaN(lat) || !double.IsNaN(lon) || !double.IsNaN(radius))))
            {
                return BadRequestError("Parameters moving, lat, lon and radius cannot be active at the same time.");
            }

            if (moving != null && ids.Length > 0)
            {
                return BadRequestError("Parameters moving and ids cannot be active at the same time.");
            }

            if (!double.IsNaN(lat) || !double.IsNaN(lon) || !double.IsNaN(radius)) // Location based search can only search for moving trains
                moving = true;


            // In case we have ids


            if (ids.Length > 0)
            {
                List<int> mysqlIDs = new List<int>();
                List<string> stringIDs = new List<string>();

                foreach (string id in ids)
                {
                    if (int.TryParse(id, out int mysqlID))
                        mysqlIDs.Add(mysqlID);
                    else
                        stringIDs.Add(id);
                }
                List<Train> allTrains = DatabaseLegacy.GetTrainsFilter(mysqlIDs.ToArray(), stringIDs.ToArray());

                JObject response = new JObject();
                foreach (string id in ids)
                {
                    Train train = allTrains.Find((t) => t.ID.ToString() == id || t.ElviraID == id);
                    response[id] = Train(train, dataFilter);
                }

                return Success(response);
            }
            else if (!double.IsNaN(lat) || !double.IsNaN(lon) || !double.IsNaN(radius))
            {
                List<Train> trains = DatabaseLegacy.GetTrains(moving);
                List<(double distance, Train train)> orderedTrains = new List<(double distance, Train train)>();
                double kmpp = WebMercator.DefaultMap.MeterPerUnit() / 1000;

                foreach (Train train in trains)
                {
                    double dist = (WebMercator.DefaultMap.FromLatLon(train.GPSPosition) - WebMercator.DefaultMap.FromLatLon(new Vector2(lat, lon))).Length * kmpp;
                    if (dist <= radius)
                    {
                        // Implementing a standard insertion sort
                        int i;
                        for (i = 0; i < orderedTrains.Count; i++)
                        {
                            if (orderedTrains[i].distance > dist) break;
                        }

                        orderedTrains.Insert(i, (dist, train));
                    }
                }

                JArray response = new JArray();
                foreach ((double distance, Train train) in orderedTrains)
                {
                    JObject trainObj = Train(train, dataFilter);
                    trainObj["distance"] = distance;
                    response.Add(trainObj);
                }
                return Success(response);
            }
            else
            {
                List<Train> trains = DatabaseLegacy.GetTrains(moving);
                JArray response = new JArray();
                foreach (Train train in trains)
                {
                    response.Add(Train(train, dataFilter));
                }
                return Success(response);
            }
        }
    }
}
