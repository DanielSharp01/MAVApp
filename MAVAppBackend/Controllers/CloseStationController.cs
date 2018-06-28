using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.Controller
{
    [Route("close-stations")]
    public class CloseStationController : APIController
    {
        [HttpGet]
        public IActionResult Get(double lat = double.NaN, double lon = double.NaN, double radius = double.NaN)
        {
            if (double.IsNaN(lat) || double.IsNaN(lon) || double.IsNaN(radius))
                return BadRequestError("Parameters lat, lon and radius must exist and must be numbers.");

            double kmpp = WebMercator.Default.MeterPerUnit() / 1000;

            List<(double distance, Station station)> stations = new List<(double distance, Station station)>();
            foreach (Station station in DatabaseLegacy.GetAllStations())
            {
                double dist = (WebMercator.Default.FromLatLon(station.GPSCoord) - WebMercator.Default.FromLatLon(new Vector2(lat, lon))).Length * kmpp;
                if (dist <= radius)
                {
                    // Implementing a standard insertion sort
                    int i;
                    for (i = 0; i < stations.Count; i++)
                    {
                        if (stations[i].distance > dist) break;
                    }

                    stations.Insert(i, (dist, station));
                }
            }

            JArray result = new JArray();

            foreach ((double distance, Station station) in stations)
            {
                JObject so = new JObject
                {
                    ["name"] = station.Name,
                    ["gpscoord"] = station.GPSCoord == null ? null : new JObject() { ["lat"] = station.GPSCoord.X, ["lon"] = station.GPSCoord.Y },
                    ["distance"] = distance
                };
                result.Add(so);
            }

            return Success(result);
        }
    }
}
