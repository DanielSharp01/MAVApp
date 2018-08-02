using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SharpEntities;

namespace MAVAppBackend.DataAccess
{
    public static class SqlExtensions
    {
        /// <summary>
        /// Gets the value of 2 double updatedColumns converted into a Vector2 object.
        /// </summary>
        /// <param name="xCoordName">Columns name of the .X coordinate</param>
        /// <param name="yCoordName">Columns name of the .Y coordinate</param>
        /// <returns></returns>
        public static Vector2 GetVector2OrNull(this DbDataReader reader, string xCoordName, string yCoordName)
        {
            if (reader.IsDBNull(reader.GetOrdinal(xCoordName)) || reader.IsDBNull(reader.GetOrdinal(yCoordName))) return null;
            else return new Vector2(reader.GetDouble(xCoordName), reader.GetDouble(yCoordName));
        }

        /// <summary>
        /// Gets the value of a single string column storing an encoded polyline and decodes it into a Polyline object.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns></returns>
        public static Polyline GetPolylineOrNull(this DbDataReader reader, string columnName)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName))) return null;
            else return new Polyline(Polyline.DecodePoints(reader.GetString(columnName), 1e5));
        }

        public static void AddVector2(this DatabaseCommandParameters collection, string nameX, string nameY, Vector2 value)
        {
            collection.Add(nameX, value == null ? null : (object)value.X);
            collection.Add(nameY, value == null ? null : (object)value.Y);
        }

        public static void AddMultipleVector2(this DatabaseCommandParameters collection, string nameX, string nameY, IEnumerable<Vector2> values)
        {
            int i = 0;
            foreach (Vector2 value in values)
            {
                collection.AddVector2($"{nameX}_{i}", $"{nameY}_{i}", value);
                i++;
            }
        }

        public static void AddPolyline(this DatabaseCommandParameters collection, string name, Polyline value)
        {
            collection.Add(name, value == null ? null : (object)Polyline.EncodePoints(value.Points, 1e5));
        }

        public static void AddMultiplePolyline(this DatabaseCommandParameters collection, string name, IEnumerable<Polyline> values)
        {
            int i = 0;
            foreach (Polyline value in values)
            {
                collection.AddPolyline($"{name}_{i++}", value);
            }
        }
    }
}
