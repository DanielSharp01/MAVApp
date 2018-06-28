using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.APIHandlers
{
    public class TRAINHandler
    {
        private MAVTable table;
        private Polyline polyline = null;

        public TRAINHandler(Database db, JObject apiResponse)
        {
            if ((apiResponse["d"]["result"]["line"] as JArray).Count > 0)
                polyline = new Polyline(Polyline.DecodePoints(apiResponse["d"]["result"]["line"][0]["points"].ToString(), 1E5f));
        }

        private List<StationLine> lineMapping(List<Station> stations)
        {
            polyline.SegmentBetween(stations[0].GPSCoord, stations[1].GPSCoord, 0.25);
            return null;
        }

        
    }
}
