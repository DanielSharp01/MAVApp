using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.SVG
{
    public class SVGCircle : SVGObject
    {
        public Vector2 Center;
        public double Radius;

        public SVGCircle(Vector2 center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Gets the SVG string associated with this object
        /// </summary>
        public override string GetSVGString()
        {
            return $"<circle cx = \"{Center.X}\" cy = \"{Center.Y}\" r = \"{Radius}\" style=\"fill: {FillColor}; stroke: {StrokeColor}; stroke-width:{StrokeWidth}\" />";
        }
    }
}
