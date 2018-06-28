using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using MAVAppBackend.SVG;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MAVAppBackend.Controller
{
    [Route("test")]
    public class TestController : APIController
    {
        private IActionResult SVG(SVGDocument doc)
        {
            return new ContentResult
            {
                ContentType = "image/svg+xml",
                Content = doc.ToString(),
                StatusCode = 200
            };
        }

        [HttpGet]
        public IActionResult Get(string pan = null, [ModelBinder(Name = "pan-station")] string panStation = null, double zoom = WebMercator.DefaultZoom, [ModelBinder(Name = "highlight-line")] int highlightLine = -1)
        {
            Vector2 panVec = Vector2.Zero;

            if (panStation != null)
            {
                Station station = Database.Instance.StationMapper.GetByName(panStation);
                if (station != null) panVec += station.GPSCoord - WebMercator.DefaultCenter;
            }
            if (pan != null)
            {
                string[] panSplit = null;
                if ((panSplit = pan.Split(',')).Length == 2)
                {
                    panVec += new Vector2(panSplit[0].Trim(), panSplit[1].Trim());
                }
            }

            SVGDocument svg = new SVGDocument(1920, 1080);

            List<Station> allStations = Database.Instance.StationMapper.GetAll();
            List<Line> allLines = Database.Instance.LineMapper.GetAll();

            WebMercator projection = new WebMercator(WebMercator.DefaultCenter + panVec, zoom, 256);

            svg.Objects.Add(new SVGPolyline(Database.Instance.LineMapper.GetByID(0).Polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "black", StrokeWidth = 1 });

            List<StationLine> allStationLines = Database.Instance.StationLineMapper.GetAll();

            Station monorierdo = Database.Instance.StationMapper.EntityCache.Values.Where(s => s.NormalizedName == StationMapper.NormalizeName("monorierdo")).First();
            Station monor = allStations.Where(s => s.NormalizedName == StationMapper.NormalizeName("monor")).First();

            StationLine a = allStationLines.Where(sl => sl.Station == monorierdo).First();
            StationLine b = allStationLines.Where(sl => sl.Station == monor).First();

            

            StationLine c = Database.Instance.StationLineMapper.GetByID(4721);

            Console.WriteLine("Distance: " + (b.Distance - a.Distance));


            foreach (Polyline polyline in allLines.Select(l => l.Polyline))
            {
                svg.Objects.Add(new SVGPolyline(polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "#138D75", StrokeWidth = 1 });
            }

            Database.Instance.StationMapper.BeginSelectNormName(new WhereInStrategy<string, Station>());
            Station[] stations = new Station[]
            {
                Database.Instance.StationMapper.GetByName("Monor"),
                Database.Instance.StationMapper.GetByName("Monorierdő"),
                Database.Instance.StationMapper.GetByName("Ferihegy")
            };

            Database.Instance.StationMapper.EndSelectNormName();

            foreach (Station station in stations)
            {
                Console.WriteLine(station.Name);
            }

            Polyline highlightable = allLines.Where(l => l.ID == highlightLine).FirstOrDefault()?.Polyline;
            //if (highlightable == null && a.Line != null) highlightable = a.Line.Polyline.SegmentBetween(a.Station.GPSCoord, b.Station.GPSCoord, 0.25);

            if (highlightable != null) svg.Objects.Add(new SVGPolyline(highlightable.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "red", StrokeWidth = 2 });

            foreach (Station station in allStations)
            {
                svg.Objects.Add(new SVGCircle(projection.FromLatLon(station.GPSCoord) + new Vector2(1920, 1080) / 2, 2) { FillColor = "#D75412" });
            }

            svg.Objects.Add(new SVGCircle(projection.FromLatLon(WebMercator.DefaultCenter + panVec) + new Vector2(1920, 1080) / 2, 10) { StrokeColor = "gray", StrokeWidth = 2 });

            return SVG(svg);
        }
    }
}
