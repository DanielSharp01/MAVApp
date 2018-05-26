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
        public Vector2 Offset
        {
            private set;
            get;
        }

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
        /// Sets the offset of everything drawn from here on
        /// </summary>
        /// <param name="offset">Offset in the SVG file (where 0,0 is)</param>
        public void SetDrawOffset(Vector2 offset)
        {
            Offset = offset;
        }

        /// <summary>
        /// Draws a polyline into the SVG stream
        /// </summary>
        /// <param name="line">Polyline to draw</param>
        /// <param name="strokeColor">Stroke color as HTML color string</param>
        /// <param name="strokeWidth">Stroke width of the polyline</param>
        public void DrawPolyline(Polyline line, string strokeColor, double strokeWidth)
        {
            writer.Write("<polyline points=\"");
            foreach (Vector2 p in line.Points)
            {
                writer.Write((p + Offset) + " ");
            }
            writer.WriteLine($"\" style=\"fill: none; stroke: {strokeColor}; stroke-width:{strokeWidth}\" />");
        }

        /// <summary>
        /// Draws a circle into the SVG stream
        /// </summary>
        /// <param name="point">Position of the point</param>
        /// <param name="radius">Radius of the point</param>
        /// <param name="fillColor">Fill color as HTML color string</param>
        /// <param name="strokeColor">Stroke color as HTML color string</param>
        /// <param name="strokeWidth">Stroke width of the outline</param>
        public void DrawCircle(Vector2 point, double radius, string fillColor = "none", string strokeColor = "none", double strokeWidth = 0)
        {
            writer.WriteLine($"<circle cx = \"{point.X + Offset.X}\" cy = \"{point.Y + Offset.Y}\" r = \"{radius}\" style=\"fill: {fillColor}; stroke: {strokeColor}; stroke-width:{strokeWidth}\" />");
        }

        /// <summary>
        /// Draws a rectangle into the SVG stream
        /// </summary>
        /// <param name="point">Position of the center</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="fillColor">Fill color as HTML color string</param>
        /// <param name="strokeColor">Stroke color as HTML color string</param>
        /// <param name="strokeWidth">Stroke width of the outline</param>
        public void DrawRectangle(Vector2 point, double width, double height, string fillColor = "none", string strokeColor = "none", double strokeWidth = 0)
        {
            writer.WriteLine($"<rect x = \"{point.X + Offset.X - width / 2}\" y = \"{point.Y + Offset.Y - height / 2}\" width = \"{width}\" height = \"{height}\" style=\"fill: {fillColor}; stroke: {strokeColor}; stroke-width:{strokeWidth}\" />");
        }

        /// <summary>
        /// Draws a rectangle into the SVG stream
        /// </summary>
        /// <param name="point">Position of the top left of the textbox</param>
        /// <param name="color">Text color as HTML color string</param>
        /// <param name="font">Font as HTML font string</param>
        public void DrawText(Vector2 point, string color, string font)
        {
            writer.WriteLine($"<text x = \"{point.X + Offset.X}\" y = \"{point.Y + Offset.Y}\" style=\"fill: {color}; font: {font} \"  />");
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
