using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Contains functions regarding the Web Mercator projection
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Tile size of WebMercator
        /// </summary>
        public const int TILE_SIZE = 256;
        /// <summary>
        /// Zoom level of Hungary on Google maps
        /// </summary>
        public const double Zoom = 8.13;
        /// <summary>
        /// Center of Hungary on Google maps
        /// </summary>
        public static readonly Vector2 Center = new Vector2(47.1569903, 18.4769959);

        /// <summary>
        /// Converts Latitude, Longitude vector into WebMercator X, Y vector
        /// </summary>
        /// <param name="p">Latitude, Longitude vector</param>
        /// <returns>WebMercator X, Y vector</returns>
        public static Vector2 LatLonToWebMerc(Vector2 p)
        {
            return new Vector2((p.Y / 180 + 1) / 2, (Math.PI - Math.Log(Math.Tan(Math.PI / 4 + p.X * Math.PI / 360))) / (2 * Math.PI));
        }

        /// <summary>
        /// Converts WebMercator X, Y vector into Latitude, Longitude vector
        /// </summary>
        /// <param name="p">WebMercator X, Y vector</param>
        /// <returns>Latitude, Longitude vector</returns>
        public static Vector2 WebMercToLatLon(Vector2 p)
        {
            return new Vector2((Math.Atan(Math.Exp(Math.PI - 2 * Math.PI * p.Y)) - Math.PI / 4) * 360 / Math.PI, 180 * (p.X * 2 - 1));
        }

        /// <summary>
        /// Scales web mercator by predefined TILE_SIZE and 2^Zoom values
        /// </summary>
        /// <param name="p">WebMercator X, Y vector</param>
        /// <returns>Scaled WebMercator X, Y vector</returns>
        public static Vector2 WebMercToScaledWebMerc(Vector2 p)
        {
            return TILE_SIZE * Math.Pow(2, Zoom) * p;
        }

        /// <summary>
        /// Scales predefined TILE_SIZE and 2^Zoom scaled WebMercator into the original
        /// </summary>
        /// /// <param name="p">Scaled WebMercator X, Y vector</param>
        /// <returns>WebMercator X, Y vector</returns>
        public static Vector2 ScaledWebMercToWebMerc(Vector2 p)
        {
            return p / (TILE_SIZE * Math.Pow(2, Zoom));
        }

        /// <summary>
        /// Returns meters per WebMercator unit (non scaled)
        /// </summary>
        /// <param name="center">Center to take into account</param>
        public static double MeterPerWebMercUnit(Vector2 center)
        {
            return Math.Cos(center.X * Math.PI / 180) * 6371000 * 2 * Math.PI;
        }
    }
}