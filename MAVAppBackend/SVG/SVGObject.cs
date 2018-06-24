using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.SVG
{
    public abstract class SVGObject
    {
        /// <summary>
        /// Color of the SVGObject's stroke
        /// </summary>
        public string StrokeColor = "none";
        /// <summary>
        /// Fill color of the SVGObject
        /// </summary>
        public string FillColor = "none";
        /// <summary>
        /// Width of the SVGObject's stroke
        /// </summary>
        public double StrokeWidth = 0;

        /// <summary>
        /// Gets the SVG string associated with this object
        /// </summary>
        public abstract string GetSVGString();

        public override string ToString()
        {
            return GetSVGString();
        }
    }
}
