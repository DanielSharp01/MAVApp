using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SharpEntities;

namespace MAVAppBackend.DataAccess
{
    public static class DbParameterExtensions
    {
        public static void AddVector2Parameter(DbParameterCollection collection, string nameX, string nameY, Vector2 value)
        {
            DbParameters.AddParameter(collection, nameX, value == null ? null : (object)value.X);
            DbParameters.AddParameter(collection, nameY, value == null ? null : (object)value.Y);
        }

        public static void AddVector2Parameters(DbParameterCollection collection, string nameX, string nameY, IEnumerable<Vector2> values)
        {
            int i = 0;
            foreach (Vector2 value in values)
            {
                AddVector2Parameter(collection, $"{nameX}_{i}", $"{nameY}_{i}", value);
                i++;
            }
        }

        public static void AddPolylineParameter(DbParameterCollection collection, string name, Polyline value)
        {
            DbParameters.AddParameter(collection, name, value == null ? null : (object)Polyline.EncodePoints(value.Points, 1e5));
        }

        public static void AddPolylineParameters(DbParameterCollection collection, string name, IEnumerable<Polyline> values)
        {
            int i = 0;
            foreach (Polyline value in values)
            {
                AddPolylineParameter(collection, $"{name}_{i++}", value);
            }
        }
    }
}
