using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace MAVAppBackend.Controllers
{
    [Route("all-stations")]
    public class AllStationsController : APIController
    {
        private abstract class StationFilter
        {
            public abstract bool Filter(Station station);
        }

        private class DistanceFilter : StationFilter
        {
            private readonly double distance;
            private readonly Vector3 nvector;
            public double LastCalculation { get; private set; }
            public DistanceFilter(double distance, double lat, double lon)
            {
                this.distance = distance;
                nvector = GeoMath.LatLonToNVector(new Vector2(lat, lon));
            }

            public override bool Filter(Station station)
            {
                if (station.GPSCoord == null) return false;
                LastCalculation = GeoMath.DistanceBetweenNVectors(nvector, GeoMath.LatLonToNVector(station.GPSCoord));
                return LastCalculation <= distance;
            }
        }

        [HttpGet]
        public IActionResult Get(string[] filters, [ModelBinder(Name = "name-only")] bool nameOnly, [ModelBinder(Name = "order-by")] string orderBy = null)
        {
            List<Station> allStations = Database.Instance.StationMapper.GetAll();

            List<StationFilter> stationFilters = new List<StationFilter>();
            JArray errors = new JArray();
            bool hasDistanceFilter = false;
            foreach (var filter in filters)
            {
                if (filter.StartsWith("distance") && filter.Length >= 10)
                {
                    string[] @params = filter.Substring(9, filter.Length - 10).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                    if (@params.Length != 3 || !double.TryParse(@params[0], out double distance)
                                            || !double.TryParse(@params[1], out double lat)
                                            || !double.TryParse(@params[2], out double lon))
                    {
                        errors.Add($"Invalid filter: '{filter}'");
                    }
                    else
                    {
                        stationFilters.Add(new DistanceFilter(distance, lat, lon));
                        hasDistanceFilter = true;
                    }
                }
                else
                {
                    errors.Add($"Invalid filter: '{filter}'");
                }
            }

            if (orderBy != null && (orderBy != "distance" || !hasDistanceFilter) && orderBy != "name" && orderBy != "id")
                errors.Add($"Can't order by '{orderBy}'");

            if (errors.Count > 0)
                return BadRequestError(errors);

            JObject dictionary = new JObject();

            List<(double? distance, Station station)> filteredStations = new List<(double? distance, Station station)>();

            foreach (var station in allStations)
            {
                bool filtered = false;
                double? distance = null;
                foreach (var filter in stationFilters)
                {
                    if (!filter.Filter(station))
                    {
                        filtered = true;
                    }

                    if (filter is DistanceFilter distFilter)
                    {
                        distance = distFilter.LastCalculation;
                    }
                }

                if (!filtered) filteredStations.Add((distance, station));
            }

            switch (orderBy)
            {
                case "distance":
                    filteredStations = filteredStations.OrderBy(i => i.distance.Value).ToList();
                    break;
                case "id":
                    filteredStations = filteredStations.OrderBy(i => i.station.Key).ToList();
                    break;
                case "name":
                    filteredStations = filteredStations.OrderBy(i => i.station.Name).ToList();
                    break;
            }

            foreach ((double? distance, Station station) in filteredStations)
            {
                if (distance.HasValue)
                {
                    JObject stationObject = station.ToJObject();
                    stationObject["distance"] = distance.Value;
                    dictionary[station.Key.ToString()] = nameOnly ? (JToken)station.Name : stationObject;
                }
                else
                {
                    dictionary[station.Key.ToString()] = nameOnly ? (JToken)station.Name : station.ToJObject();
                }
            }

            return Success(dictionary);
        }
    }
}

