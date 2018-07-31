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
using MAVAppBackend.DataAccess;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class StationMapper : UpdatableEntityMapper<int, Station>
    {
        private readonly SelectQuery baseQuery;

        public StationMapper(DbConnection connection)
            : base(connection, new Dictionary<int, Station>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("stations");
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

        protected override void InsertEntities(IList<Station> entities)
        {
            if (entities.Count == 0) return;

            DbCommand command = SqlQuery.Insert().Columns(new[] { "id", "name", "norm_name", "lat", "lon" }).Into("stations").Values(entities.Count())
                .OnDuplicateKey(new[] { "id" }).IgnoreUpdate().ToPreparedCommand(connection);

            DbParameters.AddParameters(command.Parameters, "@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            DbParameters.AddParameters(command.Parameters, "@name", entities.Select(e => e.Name));
            DbParameters.AddParameters(command.Parameters, "@norm_name", entities.Select(e => e.NormalizedName));
            DbParameterExtensions.AddVector2Parameters(command.Parameters, "@lat", "@lon", entities.Select(e => e.GPSCoord));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            throw new NotImplementedException();
        }
    }

    public class StationNNKeyMapper : UpdatableEntityMapper<string, StationNNKey>
    {
        private readonly SelectQuery baseQuery;

        public StationNNKeyMapper(DbConnection connection)
            : base(connection, new Dictionary<string, StationNNKey>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("stations");
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

        protected override void InsertEntities(IList<StationNNKey> entities)
        {
            if (entities.Count == 0) return;

            DbCommand command = SqlQuery.Insert().Columns(new[] { "id", "name", "norm_name", "lat", "lon" }).Into("stations").Values(entities.Count)
                .OnDuplicateKey(new[] { "norm_name" }).IgnoreUpdate().ToPreparedCommand(connection);

            DbParameters.AddParameters(command.Parameters, "@norm_name", entities.Select(e => e.Key));
            DbParameters.AddParameters(command.Parameters, "@id", entities.Select(e => e.ID == -1 ? null : (object)e.ID));
            DbParameters.AddParameters(command.Parameters, "@name", entities.Select(e => e.Name));
            DbParameterExtensions.AddVector2Parameters(command.Parameters, "@lat", "@lon", entities.Select(e => e.GPSCoord));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<string> keys)
        {
            throw new NotImplementedException();
        }
    }
}
