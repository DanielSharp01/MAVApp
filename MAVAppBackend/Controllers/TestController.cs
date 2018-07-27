using System.Collections.Generic;
using System.Linq;
using MAVAppBackend.APIHandlers;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using MAVAppBackend.SVG;
using Microsoft.AspNetCore.Mvc;

namespace MAVAppBackend.Controllers
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
        public IActionResult Get(string pan = null, [ModelBinder(Name = "pan-station")] string panStation = null, double zoom = WebMercator.DefaultZoom, [ModelBinder(Name = "line")] int[] highlightLine = null,
            [ModelBinder(Name="train")]int trainId = -1)
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

            svg.Objects.Add(new SVGPolyline(Database.Instance.LineMapper.GetByKey(0).Polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "rgba(0, 0, 0, 0.3)", StrokeWidth = 1 });

            foreach (Polyline polyline in allLines.Select(l => l.Polyline))
            {
                svg.Objects.Add(new SVGPolyline(polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "#138D75", StrokeWidth = 1 });
            }

            List<Polyline> highlightables = allLines.Where(l => highlightLine.Contains(l.Key)).Select(l => l.Polyline).ToList();

            foreach (Polyline highlightable in highlightables)
            {
                svg.Objects.Add(new SVGPolyline(highlightable.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "red", StrokeWidth = 2 });
            }

            if (trainId != -1)
            {
                TRAINHandler apiHandler = new TRAINHandler(MAVAPI.RequestTrain(trainId));
                apiHandler.LineMapping();
                svg.Objects.Add(new SVGPolyline(apiHandler.polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "rgba(0, 0, 255, 0.5)", StrokeWidth = 2 });
            }

            /*foreach (Station station in allStations)
            {
                if (station.GPSCoord != null)
                    svg.Objects.Add(new SVGCircle(projection.FromLatLon(station.GPSCoord) + new Vector2(1920, 1080) / 2, 2) { FillColor = "#D75412" });
            }*/

            svg.Objects.Add(new SVGCircle(projection.FromLatLon(WebMercator.DefaultCenter + panVec) + new Vector2(1920, 1080) / 2, 10) { StrokeColor = "gray", StrokeWidth = 2 });

            return SVG(svg);
        }
    }
}
