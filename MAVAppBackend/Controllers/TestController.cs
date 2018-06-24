using MAVAppBackend.SVG;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MAVAppBackend
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
        public IActionResult Get()
        {
            //MySqlCommand cmd = new MySqlCommand("", Database.connection);

            SVGDocument svg = new SVGDocument(1920, 1080);

            using (FileStream stream = new FileStream("_terkep.kml", FileMode.Open))
            {
                XDocument doc = XDocument.Load(stream);
                foreach (XElement point_elem in doc.Root.Descendants().Where(e => e.Name.LocalName == "coordinates"))
                {
                    List<Vector2> points = point_elem.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p =>
                    { string[] c = p.Split(',', StringSplitOptions.RemoveEmptyEntries); return new Vector2(double.Parse(c[1]), double.Parse(c[0])); }).ToList();

                    points = points.ConvertAll(c => Map.DefaultMap.FromLatLon(c) + new Vector2(1920 / 2, 1080 / 2));
                    svg.Objects.Add(new SVGPolyline(points) { StrokeColor = "black", StrokeWidth = 1});
                }
            }
            

            return SVG(svg);
        }
    }
}
