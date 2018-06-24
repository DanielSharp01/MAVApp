using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend.SVG
{
    public class SVGDocument
    {
        /// <summary>
        /// List of SVG Objects add to this to see changes
        /// </summary>
        public List<SVGObject> Objects = new List<SVGObject>();

        /// <summary>
        /// Width of the SVG canvas
        /// </summary>
        public int Width { private set; get;}
        /// <summary>
        /// Height of the SVG canvas
        /// </summary>
        public int Height { private set; get; }

        /// <param name="width">Width of the SVG canvas</param>
        /// <param name="height">Height of the SVG canvas</param>
        public SVGDocument(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"{Width}\" height=\"{Height}\">");
            foreach (SVGObject o in Objects)
            {
                builder.Append(o.ToString());
            }
            builder.Append("</svg>");
            return builder.ToString();
        }
    }
}
