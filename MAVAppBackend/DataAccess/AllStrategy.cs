using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public class AllStrategy<K, E> : BatchSelectStrategy<K, E> where E : Entity
    {
        private Dictionary<K, E> entities = new Dictionary<K, E>();
        public override void AddEntity(K key, E entity)
        {
            entities.TryAdd(key, entity);
        }

        public override void BatchFill(MySqlConnection connection, SelectQuery unfilteredQuery, string column, Func<E, MySqlDataReader, bool> fillMethod)
        {
            if (entities.Count == 0) return;

            MySqlCommand cmd = unfilteredQuery.ToCommand(connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                K key = (K)reader.GetValue(reader.GetOrdinal(column));
                while ((!entities.ContainsKey(key) && reader.Read()) || (entities.ContainsKey(key) && fillMethod(entities[key], reader)))
                {
                    key = (K)reader.GetValue(reader.GetOrdinal(column));
                }
            }

            reader.Close();

        }
    }
}
