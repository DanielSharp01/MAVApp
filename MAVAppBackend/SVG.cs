using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// SVG FileStream which you can draw on
    /// </summary>
    public class SVGStream
    {
        /// <summary>
        /// Writer writing to the SVG file
        /// </summary>
        private StreamWriter writer;

        /// <summary>
        /// Opens a StreamWriter into the specified file and writes the SVG header
        /// </summary>
        /// <param name="filename">SVG file</param>
        /// <param name="width">Width of the SVG file</param>
        /// <param name="height">Height of the SVG file</param>
        public SVGStream(string filename, int width, int height)
        {
            writer = new StreamWriter(filename);
            writer.Write($"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"{width}\" height=\"{height}\">");
        }

        /// <summary>
        /// Draws a polyline into the SVG stream
        /// </summary>
        /// <param name="line">Polyline to draw</param>
        /// <param name="offset">Offset in the SVG file (where 0,0 is)</param>
        /// <param name="htmlColor">Stroke color as HTML color string</param>
        /// <param name="strokeWidth">Stroke width of the polyline</param>
        public void DrawPolyline(Polyline line, Vector2 offset, string htmlColor, double strokeWidth)
        {
            writer.Write("<polyline points=\"");
            foreach (Vector2 p in line.Points)
            {
                writer.Write((p + offset) + " ");
            }
            writer.WriteLine($"\" style=\"fill: none; stroke: {htmlColor}; stroke-width:{strokeWidth}\" />");
        }

        /// <summary>
        /// Draws a point (filled circle) into the SVG stream
        /// </summary>
        /// <param name="point">Position of the point</param>
        /// <param name="offset">Offset in the SVG file (where 0,0 is)</param>
        /// <param name="htmlColor">Fill color as HTML color string</param>
        /// <param name="radius">Radius of the point</param>
        public void DrawPoint(Vector2 point, Vector2 offset, string htmlColor, double radius)
        {
            writer.WriteLine($"<circle cx = \"{point.X + offset.X}\" cy = \"{point.Y + offset.Y}\" r = \"{radius}\" style=\"fill: {htmlColor}\" />");
        }

        /// <summary>
        /// Draws a (non-filled) circle into the SVG stream
        /// </summary>
        /// <param name="point">Position of the center</param>
        /// <param name="offset">Offset in the SVG file (where 0,0 is)</param>
        /// <param name="htmlColor">Stroke color as HTML color string</param>
        /// <param name="radius">Radius of the circle</param>
        public void DrawCircle(Vector2 point, Vector2 offset, double radius, string htmlColor, double strokeWidth)
        {
            writer.WriteLine($"<circle cx = \"{point.X + offset.X}\" cy = \"{point.Y + offset.Y}\" r = \"{radius }\" style=\"fill: none; stroke: {htmlColor}; stroke-width:{strokeWidth}\" />");
        }

        /// <summary>
        /// Ends the SVG tag and closes the SVG stream
        /// </summary>
        public void Close()
        {
            writer.WriteLine("</svg>");
            writer.Close();
        }
    }
}
