using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Contains functions regarding the Web Mercator projection. The object instance represents a specific WebMercator projection.
    /// Terminology: When talking about an unscaled WebMercator I mean the projection given by new WebMercator(DefaultCenter, 0, 1), Hungarian default offset, no zoom and no tile scaling.
    /// </summary>
    public class WebMercator
    {
        /// <summary>
        /// Tile size of Google Maps
        /// </summary>
        public const int DEFAULT_TILE_SIZE = 256;
        /// <summary>
        /// Zoom level of Hungary on Google maps
        /// </summary>
        public const double DefaultZoom = 8.13;
        /// <summary>
        /// Center of Hungary on Google maps
        /// </summary>
        public static readonly Vector2 DefaultCenter = new Vector2(47.1569903, 18.4769959);

        /// <summary>
        /// Default Map used throughout the application
        /// </summary>
        public static readonly WebMercator Default = new WebMercator();

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

        public WebMercator()
        {
            Center = LatLonToUnscaled(DefaultCenter);
            Zoom = DefaultZoom;
            TileSize = DEFAULT_TILE_SIZE;
        }

        /// <param name="center">Center of the map in latitude, longitude</param>
        /// <param name="zoom"></param>
        /// <param name="tileSize"></param>
        public WebMercator(Vector2 center, double zoom = DefaultZoom, int tileSize = DEFAULT_TILE_SIZE)
        {
            Center = LatLonToUnscaled(center);
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
            return FromUnscaled(LatLonToUnscaled(p));
        }

        /// <summary>
        /// Projects a WebMercator X, Y vector into a different center, zoom and tile size
        /// </summary>
        /// <param name="p">WebMercator X, Y vector</param>
        /// <param name="center">Latitude, longitude vector of the center of the projection</param>
        /// <param name="zoom">Zoom of the projection</param>
        /// <param name="center">Tile size of the projection</param>
        /// <returns>WebMercator X, Y vector in the new projection</returns>
        public Vector2 Project(Vector2 p, WebMercator map)
        {
            return map.FromUnscaled(ToUnscaled(p));
        }

        /// <summary>
        /// Projects an unscaled WebMecator into this projection
        /// </summary>
        /// <param name="p">Uncaled WebMercator X, Y vector</param>
        /// <returns>WebMercator X, Y vector in the new projection</returns>
        public Vector2 FromUnscaled(Vector2 p)
        {
            return (p - Center) * TileSize * Math.Pow(2, Zoom);
        }

        /// <summary>
        /// Projects this map's WebMercator vector into an unscaled WebMercator vector
        /// </summary>
        /// <param name="p">WebMercator X, Y in this map's projection</param>
        /// <returns>Unscaled WebMercator X, Y vector</returns>
        public Vector2 ToUnscaled(Vector2 p)
        {
            return p / TileSize / Math.Pow(2, Zoom) + Center;
        }

        /// <summary>
        /// Converts WebMercator X, Y vector into Latitude, Longitude vector
        /// </summary>
        /// <param name="p">WebMercator X, Y vector</param>
        /// <returns>Latitude, Longitude vector</returns>
        public Vector2 ToLatLon(Vector2 p)
        {
            return UnscaledToLatLon(ToUnscaled(p));
        }

        /// <summary>
        /// Returns meters per WebMercator unit
        /// </summary>
        public double MeterPerUnit()
        {
            return Math.Cos(ToLatLon(Center).X * Math.PI / 180) * 6378137 * 2 * Math.PI / (TileSize * Math.Pow(2, Zoom));
        }

        /// <summary>
        /// Converts Latitude, Longitude vector into Unscaled WebMercator X, Y vector
        /// </summary>
        /// <param name="p">Latitude, Longitude vector</param>
        /// <returns>Unscaled WebMercator X, Y vector</returns>
        public static Vector2 LatLonToUnscaled(Vector2 p)
        {
            return new Vector2((p.Y / 180 + 1) / 2, (Math.PI - Math.Log(Math.Tan(Math.PI / 4 + p.X * Math.PI / 360))) / (2 * Math.PI));
        }

        /// <summary>
        /// Converts Unscaled WebMercator X, Y vector into Latitude, Longitude vector
        /// </summary>
        /// <param name="p">Unscaled WebMercator X, Y vector</param>
        /// <returns>Latitude, Longitude vector</returns>
        public static Vector2 UnscaledToLatLon(Vector2 p)
        {
            return new Vector2((Math.Atan(Math.Exp(Math.PI - 2 * Math.PI * p.Y)) - Math.PI / 4) * 360 / Math.PI, 180 * (p.X * 2 - 1));
        }
    }
}