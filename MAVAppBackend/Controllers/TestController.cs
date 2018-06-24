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
            
            
            //MySqlConnection conn = new MySqlConnection("Host=127.0.0.1;Database=mavapp;UserName=root;Password=mysql");
            //conn.Open();

            //StringBuilder values = new StringBuilder();
            //for (int i = 0; i < borderPoints.Count; i++)
            //{
            //    values.Append($"(@id_{i}, @number_{i}, @lat_{i}, @lon_{i})");
            //    if (i < borderPoints.Count - 1) values.Append(", ");
            //}

            //MySqlCommand cmd = new MySqlCommand($"INSERT INTO line_points VALUES {values.ToString()}", conn);
            //cmd.Prepare();


            //for (int i = 0; i < borderPoints.Count; i++)
            //{
            //    cmd.Parameters.AddWithValue($"id_{i}", 0);
            //    cmd.Parameters.AddWithValue($"number_{i}", i + 1);
            //    cmd.Parameters.AddVector2WithValue($"lat_{i}", $"lon_{i}", borderPoints[i]);
            //}
            //cmd.ExecuteNonQuery();
            //conn.Close();

            WebMercator projection = new WebMercator(WebMercator.DefaultCenter + panVec, zoom, 256);

            svg.Objects.Add(new SVGPolyline(Database.Instance.LineMapper.GetByID(0).Polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "black", StrokeWidth = 1 });

            foreach (Line line in Database.Instance.LineMapper.GetAll())
            {
                svg.Objects.Add(new SVGPolyline(line.Polyline.Points.Select(p => projection.FromLatLon(p) + new Vector2(1920, 1080) / 2).ToList()) { StrokeColor = "#138D75", StrokeWidth = 1 });
            }

            foreach (Station station in Database.Instance.StationMapper.GetAll())
            {
                svg.Objects.Add(new SVGCircle(projection.FromLatLon(station.GPSCoord) + new Vector2(1920, 1080) / 2, 2) { FillColor = "#D75412" });
            }

            svg.Objects.Add(new SVGCircle(projection.FromLatLon(WebMercator.DefaultCenter + panVec) + new Vector2(1920, 1080) / 2, 10) { StrokeColor = "gray", StrokeWidth = 2 });

            return SVG(svg);
        }
    }
}
