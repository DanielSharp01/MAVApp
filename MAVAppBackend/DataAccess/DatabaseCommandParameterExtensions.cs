using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SharpEntities;

namespace MAVAppBackend.DataAccess
{
    public static class DatabaseCommandParametersExtensions
    {
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
