using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public abstract class BatchSelectStrategy<K, E> where E : Entity
    {
        public abstract void AddEntity(K key, E entity);

        public abstract void BatchFill(MySqlConnection connection, SelectQuery unfilteredQuery, string column, Func<E, MySqlDataReader, bool> fillMethod);
    }
}
