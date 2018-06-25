using MAVAppBackend.DataAccess;
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
        public IActionResult Get(string pan = null, [ModelBinder(Name = "pan-station")] string panStation = null, double zoom = WebMercator.DefaultZoom)
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

            List<(Station station, Line line, double dist)> relationalTable = new List<(Station station, Line line, double dist)>();

            foreach (Line line in allLines)
            {
                foreach (Station station in allStations)
                {
                    double dist = line.Polyline.GetProjectedDistance(station.GPSCoord, 0.2);

                    if (!double.IsNaN(dist))
                    {
                        relationalTable.Add((station, line, dist));
                    }
                }
            }

            foreach (Line line in allLines)
            {
                svg.Objects.Add(new SVGPolyline(line.Polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "#138D75", StrokeWidth = 1 });
            }

            foreach (Station station in allStations)
            {
                svg.Objects.Add(new SVGCircle(projection.FromLatLon(station.GPSCoord) + new Vector2(1920, 1080) / 2, 2) { FillColor = "#D75412" });
            }

            foreach ((Station station, Line line, double dist) in relationalTable)
            {
                Console.WriteLine(station.Name + " on line " + line.ID + " @ distance " + dist);
            }


            MySqlConnection conn = new MySqlConnection("Host=127.0.0.1;Database=mavapp;UserName=root;Password=mysql");
            conn.Open();

            StringBuilder values = new StringBuilder();
            for (int i = 0; i < relationalTable.Count; i++)
            {
                values.Append($"(NULL, @station_{i}, @line_{i}, @dist_{i})");
                if (i < relationalTable.Count - 1) values.Append(", ");
            }

            MySqlCommand cmd = new MySqlCommand($"INSERT INTO station_line VALUES {values.ToString()}", conn);
            cmd.Prepare();


            for (int i = 0; i < relationalTable.Count; i++)
            {
                cmd.Parameters.AddWithValue($"station_{i}", relationalTable[i].station.ID);
                cmd.Parameters.AddWithValue($"line_{i}", relationalTable[i].line.ID);
                cmd.Parameters.AddWithValue($"dist_{i}", relationalTable[i].dist);
            }
            cmd.ExecuteNonQuery();
            conn.Close();


            return SVG(svg);
        }
    }
}
