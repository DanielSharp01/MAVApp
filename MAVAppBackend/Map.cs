using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Contains functions regarding the Web Mercator projection. The object instance represents a specific WebMercator projection.
    /// Terminology: When talking about an unscaled WebMercator I mean the projection given by new Map(new Vector2(0, 0), 1, 1), no offset, no zoom and no tile scaling.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Tile size of Google Maps
        /// </summary>
        public const int TILE_SIZE = 256;
        /// <summary>
        /// Zoom level of Hungary on Google maps
        /// </summary>
        public const double DefaultZoom = 8.13;
        /// <summary>
        /// Center of Hungary on Google maps
        /// </summary>
        public static readonly Vector2 DefaultCenter = new Vector2(47.1569903, 18.4769959);

        /// <summary>
        /// The unscaled map
        /// </summary>
        public static readonly Map UnscaledMap = new Map(new Vector2(0, 0), 1, 1);

        /// <summary>
        /// Default Map used throughout the application
        /// </summary>
        public static readonly Map DefaultMap = new Map();

        /// <summary>
        /// Tile size of this map
        /// </summary>
        public int TileSize
        {
            private set;
            get;
        }

        /// <summary>
        /// Zoom level of this map
        /// </summary>
        public double Zoom
        {
            private set;
            get;
        }

        /// <summary>
        /// Center of this map in unscaled web mercator
        /// </summary>
        public Vector2 Center
        {
            private set;
            get;
        }

        public Map()
        {
            Center = LatLonToWebMerc(DefaultCenter);
            Zoom = DefaultZoom;
            TileSize = TILE_SIZE;
        }

        /// <param name="center">Center of the map in latitude, longitude</param>
        /// <param name="zoom"></param>
        /// <param name="tileSize"></param>
        public Map(Vector2 center, double zoom = DefaultZoom, int tileSize = TILE_SIZE)
        {
            Center = LatLonToWebMerc(center);
            Zoom = zoom;
            TileSize = tileSize;
        }

        /// <summary>
        /// Converts Latitude, Longitude vector into WebMercator X, Y vector
        /// </summary>
        /// <param name="p">Latitude, Longitude vector</param>
        /// <returns>WebMercator X, Y vector</returns>
        public Vector2 FromLatLon(Vector2 p)
        {
            return ProjectUnscaled(LatLonToWebMerc(p));
        }

        /// <summary>
        /// Reprojects a WebMercator X, Y vector into a different center, zoom and tile size
        /// </summary>
        /// <param name="p">WebMercator X, Y vector</param>
        /// <param name="center">Latitude, longitude vector of the center of the projection</param>
        /// <param name="zoom">Zoom of the projection</param>
        /// <param name="center">Tile size of the projection</param>
        /// <returns>WebMercator X, Y vector in the new projection</returns>
        public Vector2 Reproject(Vector2 p, Map map)
        {
            return map.ProjectUnscaled(IntoUnscaled(p));
        }

        /// <summary>
        /// Projects an unscaled WebMecator into this projection
        /// </summary>
        /// <param name="p">Uncaled WebMercator X, Y vector</param>
        /// <returns>WebMercator X, Y vector in the new projection</returns>
        public Vector2 ProjectUnscaled(Vector2 p)
        {
            return (p - Center) * TILE_SIZE * Math.Pow(2, Zoom);
        }

        /// <summary>
        /// Projects this map's WebMercator vector into an unscaled WebMercator vector
        /// </summary>
        /// <param name="p">WebMercator X, Y in this map's projection</param>
        /// <returns>Unscaled WebMercator X, Y vector</returns>
        public Vector2 IntoUnscaled(Vector2 p)
        {
            return p / TILE_SIZE / Math.Pow(2, Zoom) + Center;
        }

        /// <summary>
        /// Converts WebMercator X, Y vector into Latitude, Longitude vector
        /// </summary>
        /// <param name="p">WebMercator X, Y vector</param>
        /// <returns>Latitude, Longitude vector</returns>
        public Vector2 ToLatLon(Vector2 p)
        {
            return WebMercToLatLon(IntoUnscaled(p));
        }

        /// <summary>
        /// Returns meters per WebMercator unit (non scaled)
        /// </summary>
        /// <param name="center">Center to take into account</param>
        public double MeterPerWebMercUnit()
        {
            return Math.Cos(Center.X * Math.PI / 180) * 6371000 * 2 * Math.PI / (TILE_SIZE * Math.Pow(2, Zoom));
        }

        /// <summary>
        /// Converts Latitude, Longitude vector into non scaled/zoomed WebMercator X, Y vector
        /// </summary>
        /// <param name="p">Latitude, Longitude vector</param>
        /// <returns>Non scaled/zoomed WebMercator X, Y vector</returns>
        public static Vector2 LatLonToWebMerc(Vector2 p)
        {
            return new Vector2((p.Y / 180 + 1) / 2, (Math.PI - Math.Log(Math.Tan(Math.PI / 4 + p.X * Math.PI / 360))) / (2 * Math.PI));
        }

        /// <summary>
        /// Converts non scaled/zoomed WebMercator X, Y vector into Latitude, Longitude vector
        /// </summary>
        /// <param name="p">Non scaled/zoomed WebMercator X, Y vector</param>
        /// <returns>Latitude, Longitude vector</returns>
        public static Vector2 WebMercToLatLon(Vector2 p)
        {
            return new Vector2((p.Y / 180 + 1) / 2, (Math.PI - Math.Log(Math.Tan(Math.PI / 4 + p.X * Math.PI / 360))) / (2 * Math.PI));
        }
    }
}