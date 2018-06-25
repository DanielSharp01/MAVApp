using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    public enum TrainDataFilter
    {
        StaticOnly,
        DynamicOnly,
        Both
    }
    public class APIController : ControllerBase
    {
        public bool TryParseTrainDataFilter(string data, out TrainDataFilter dataFilter)
        {
            dataFilter = TrainDataFilter.Both;
            switch (data)
            {
                case "static only": dataFilter = TrainDataFilter.StaticOnly; break;
                case "dynamic only": dataFilter = TrainDataFilter.DynamicOnly; break;
                case "both": dataFilter = TrainDataFilter.Both; break;
                default: return false;
            }

            return true;
        }
        public JObject Train(Train train, TrainDataFilter dataFilter)
        {
            JObject obj = new JObject();
            if (train == null)
                return null;

            obj["id"] = train.ID;
            obj["elvira-id"] = train.ElviraID;
            if (dataFilter != TrainDataFilter.DynamicOnly)
            {
                obj["number"] = train.Number;
                obj["name"] = train.Name;
                obj["type"] = train.Type;
                obj["number-type"] = train.NumberType;
                obj["delay-reason"] = train.DelayReason;
                obj["misc-info"] = new JArray(train.MiscInfo.ToArray());
                obj["enc-polyline"] = train.Polyline == null ? null : Polyline.EncodePoints(train.Polyline.Points.ToList(), 1E5f);
                obj["stations"] = new JArray();
                JArray arr = obj["stations"] as JArray;
                foreach (StationInfo station in train.Stations)
                {
                    JObject so = new JObject();
                    so["name"] = station.Name;
                    so["int-distance"] = station.IntDistance;
                    so["distance"] = station.Distance;
                    switch (station.PositionAccuracy)
                    {
                        case StationPositionAccuracy.Missing:
                            so["position-accuracy"] = "missing";
                            break;
                        case StationPositionAccuracy.IntegerPrecision:
                            so["position-accuracy"] = "integer-precision";
                            break;
                        case StationPositionAccuracy.Precise:
                            so["position-accuracy"] = "precise";
                            break;
                    }
                    so["arrival"] = station.Arrival.ToString("yyyy-MM-dd HH:mm:ss");
                    so["departure"] = station.Departure.ToString("yyyy-MM-dd HH:mm:ss");
                    so["actual-arrival"] = station.ExpectedArrival.ToString("yyyy-MM-dd HH:mm:ss");
                    so["actual-departure"] = station.ExpectedDeparture.ToString("yyyy-MM-dd HH:mm:ss");
                    so["arrived"] = station.Arrived;
                    so["platform"] = station.Platform;
                    arr.Add(so);
                }
            }
            if (dataFilter != TrainDataFilter.StaticOnly)
            {
                obj["delay"] = train.Delay;
                obj["gpscoord"] = train.GPSPosition == null ? null : new JObject() { ["lat"] = train.GPSPosition.X, ["lon"] = train.GPSPosition.Y };
                obj["last-gpscoord"] = train.LastGPSPosition == null ? null : new JObject() { ["lat"] = train.LastGPSPosition.X, ["lon"] = train.LastGPSPosition.Y };
            }

            return obj;
        }

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
