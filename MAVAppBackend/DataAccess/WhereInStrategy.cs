using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public class WhereInStrategy<K, E> : BatchSelectStrategy<K, E> where E : Entity
    {
        private Dictionary<K, E> entities = new Dictionary<K, E>();
        public override void AddEntity(K key, E entity)
        {
            entities.TryAdd(key, entity);
        }

        public override void BatchFill(MySqlConnection connection, SelectQuery unfilteredQuery, string column, Func<E, MySqlDataReader, bool> fillMethod)
        {
            if (entities.Count == 0) return;

            if (entities.Count == 1)
            {
                MySqlCommand cmd = unfilteredQuery.Where($"{column} = @id").ToPreparedCommand(connection);
                cmd.Parameters.AddWithValue("@id", entities.Values.First().ID);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    fillMethod(entities.Values.First(), reader);
                }

                reader.Close();
            }
            else
            {
                MySqlCommand cmd = unfilteredQuery.WhereIn(column, "@id", entities.Values.Count).ToPreparedCommand(connection);
                cmd.Parameters.AddWithValues("@id", entities.Values.Select(e => e.ID));
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    while (fillMethod(entities[(K)reader.GetValue(reader.GetOrdinal(column))], reader)) ;
                }

                reader.Close();
            }
            
        }
    }
}
