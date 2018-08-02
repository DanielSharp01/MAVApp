using System;
using System.Collections.Generic;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace SharpEntities
{
    public abstract class DatabaseCommand : IDisposable
    {
        public abstract DatabaseCommandParameters Parameters { get; }

        protected DbConnection connection;

        protected DatabaseCommand(DbConnection connection)
        {
            this.connection = connection;
        }

        public abstract DbDataReader ExecuteReader();
        public abstract int ExecuteNonQuery();
        public abstract void Dispose();
    }

    public abstract class DatabaseCommandParameters
    {
        protected DatabaseCommandParameters(DbParameterCollection collection) { }

        public abstract IEnumerable<(string key, object parameter)> Parameters { get; }

        public abstract void Add<T>(string key, T parameter);

        public void AddMultiple<T>(string key, IEnumerable<T> parameters)
        {
            int i = 0;
            foreach (var parameter in parameters)
            {
                Add($"{key}_{i}", parameter);
                i++;
            }
        }

        public abstract void Clear();

        public abstract void Clear(string key);
    }
}
