using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.SVG
{
    public class SVGRectangle : SVGObject
    {
        /// <summary>
        /// Top left X coordinate
        /// </summary>
        public double Left;
        /// <summary>
        /// Top left Y coordinate
        /// </summary>
        public double Top;

        public Vector2 Center
        {
            get
            {
                return new Vector2(Left + Width / 2, Top + Height / 2);
            }

            set
            {
                Left = value.X - Width / 2;
                Top = value.Y - Height / 2;
            }
        }

        public double Width;
        public double Height;

        /// <param name="left">Top left X coordinate</param>
        /// <param name="top">Top left Y coordinate</param>
        public SVGRectangle(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public SVGRectangle(Vector2 center, double width, double height)
        {
            Center = center;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the SVG string associated with this object
        /// </summary>
        public override string GetSVGString()
        {
            return $"<rect x = \"{Left}\" y = \"{Top}\" width = \"{Width}\" height = \"{Height}\" style=\"fill: {FillColor}; stroke: {StrokeColor}; stroke-width:{StrokeWidth}\" />";
        }
    }
}
