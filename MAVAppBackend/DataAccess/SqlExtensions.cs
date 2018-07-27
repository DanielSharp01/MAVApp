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
    }
}
