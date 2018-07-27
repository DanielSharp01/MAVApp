using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;

namespace SharpEntities
{
    public static class DbParameters
    {
        public static void AddParameter(DbParameterCollection collection, string name, object value)
        {
            if (collection is MySqlParameterCollection mySqlCollection)
            {
                mySqlCollection.AddWithValue(name, value);
            }
        }

        public static void AddParameters<T>(DbParameterCollection collection, string name, IEnumerable<T> values)
        {
            int i = 0;
            foreach (T value in values)
            {
                AddParameter(collection, $"{name}_{i++}", value);
            }
        }
    }
}
