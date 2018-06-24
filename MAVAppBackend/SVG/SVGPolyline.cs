using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend.SVG
{
    public class SVGPolyline : SVGObject
    {
        public List<Vector2> Points;

        /// <summary>
        /// Whether the polyline is closed (repeats it's first point at the end)
        /// </summary>
        public bool Closed;

        /// <param name="points">Points of the polyline</param>
        /// <param name="closed">Whether the polyline is closed (repeats it's first point at the end)</param>
        public SVGPolyline(List<Vector2> points, bool closed = false)
        {
            Points = points;
            Closed = closed;
        }

        /// <summary>
        /// Gets the SVG string associated with this object
        /// </summary>
        public override string GetSVGString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<polyline points=\"");
            foreach (Vector2 p in Points)
            {
                builder.Append((p) + " ");
            }
            if (Closed) builder.Append((Points.First()) + " ");
            builder.Append($"\" style=\"fill: none; stroke: {StrokeColor}; stroke-width:{StrokeWidth}\" />");

            return builder.ToString();
        }
    }
}
