using MAVAppBackend.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class StationMapper : EntityMapper<int, Station>
    {
        private readonly SelectQuery baseQuery;

        public StationMapper(DbConnection connection)
            : base(connection, new Dictionary<int, Station>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("stations").Where("`mav_found` = 1");
        }

        protected override Station CreateEntity(int key)
        {
            return new Station(key);
        }

        private DbCommand selectByKeyCmd;
        protected override DbDataReader SelectByKey(int key)
        {
            selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`id` = @id").ToPreparedCommand(connection);
            selectByKeyCmd.Parameters.Clear();
            DbParameters.AddParameter(selectByKeyCmd.Parameters, "@id", key);
            return selectByKeyCmd.ExecuteReader();
        }

        protected override DbDataReader SelectByKeys(IEnumerable<int> keys)
        {
            var keyArray = keys as int[] ?? keys.ToArray();

            DbCommand cmd = baseQuery.Clone().WhereIn("id", keyArray.Count()).ToPreparedCommand(connection);
            cmd.Parameters.Clear();
            DbParameters.AddParameters(cmd.Parameters, "@id", keyArray);
            return cmd.ExecuteReader();
        }

        private DbCommand selectAllCmd;
        protected override DbDataReader SelectAll()
        {
            selectAllCmd = selectAllCmd ?? baseQuery.ToCommand(connection);
            return selectAllCmd.ExecuteReader();
        }

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }
    }

    public class StationNNKeyMapper : EntityMapper<string, StationNNKey>
    {
        private readonly SelectQuery baseQuery;

        public StationNNKeyMapper(DbConnection connection)
            : base(connection, new Dictionary<string, StationNNKey>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("stations").Where("`mav_found` = 1");
        }

        protected override StationNNKey CreateEntity(string key)
        {
            return new StationNNKey(key);
        }

        private DbCommand selectByKeyCmd;
        protected override DbDataReader SelectByKey(string key)
        {
            selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`norm_name` = @norm_name").ToPreparedCommand(connection);
            selectByKeyCmd.Parameters.Clear();
            DbParameters.AddParameter(selectByKeyCmd.Parameters, "@norm_name", key);
            return selectByKeyCmd.ExecuteReader();
        }

        protected override DbDataReader SelectByKeys(IEnumerable<string> keys)
        {
            var keyArray = keys as string[] ?? keys.ToArray();

            DbCommand cmd = baseQuery.Clone().WhereIn("norm_name", keyArray.Count()).ToPreparedCommand(connection);
            cmd.Parameters.Clear();
            DbParameters.AddParameters(cmd.Parameters, "@norm_name", keyArray);
            return cmd.ExecuteReader();
        }

        private DbCommand selectAllCmd;
        protected override DbDataReader SelectAll()
        {
            selectAllCmd = selectAllCmd ?? baseQuery.ToPreparedCommand(connection);
            return selectAllCmd.ExecuteReader();
        }

        protected override string GetKey(DbDataReader reader)
        {
            return reader.GetString("norm_name");
        }
    }
}
