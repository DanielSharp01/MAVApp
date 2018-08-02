using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;

namespace SharpEntities.MySql
{
    public class MySqlDatabaseCommand : DatabaseCommand
    {
        private readonly MySqlCommand command;
        private readonly MySqlDatabaseCommandParameters parameters;

        public MySqlDatabaseCommand(MySqlConnection connection, string sql, bool prepared = false)
            : base(connection)
        {
            command = new MySqlCommand(sql, (MySqlConnection)connection);
            if (prepared) command.Prepare();
            parameters = new MySqlDatabaseCommandParameters(command.Parameters);
        }

        public override DatabaseCommandParameters Parameters => parameters;

        public override DbDataReader ExecuteReader()
        {
            return command.ExecuteReader();
        }

        public override int ExecuteNonQuery()
        {
            return command.ExecuteNonQuery();
        }

        public override void Dispose()
        {
            command?.Dispose();
        }
    }

    public class MySqlDatabaseCommandParameters : DatabaseCommandParameters
    {
        private readonly MySqlParameterCollection collection;
        public MySqlDatabaseCommandParameters(MySqlParameterCollection collection)
            : base(collection)
        {
            this.collection = collection;
        }

        public override IEnumerable<(string key, object parameter)> Parameters
        {
            get
            {
                foreach (MySqlParameter parameter in collection)
                {
                    yield return (parameter.ParameterName, parameter.Value);
                }
            }
        }

        public override void Add<T>(string key, T parameter)
        {
            collection.AddWithValue(key, parameter);
        }

        public override void Clear()
        {
            collection.Clear();
        }

        public override void Clear(string key)
        {
            collection.Remove(key);
        }
    }
}
