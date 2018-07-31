using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class TrainEntityMapper : UpdatableEntityMapper<int, Train>
    {
        private readonly SelectQuery baseQuery;

        public TrainEntityMapper(DbConnection connection)
            : base(connection, new Dictionary<int, Train>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("trains");
        }

        protected override Train CreateEntity(int key)
        {
            return new Train(key);
        }

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }

        protected override void InsertEntities(IList<Train> entities)
        {
            if (entities.Count == 0) return;

            DbCommand command = SqlQuery.Insert().Columns(new[] {"id", "name", "type", "polyline", "expiry_date"}).Into("trains").Values(entities.Count())
                .OnDuplicateKey(new[] {"name", "type", "polyline", "expiry_date"}).ToPreparedCommand(connection);

            DbParameters.AddParameters(command.Parameters, "@id", entities.Select(e => e.Key));
            DbParameters.AddParameters(command.Parameters, "@name", entities.Select(e => e.Name));
            DbParameters.AddParameters(command.Parameters, "@type", entities.Select(e => e.Type));
            DbParameterExtensions.AddPolylineParameters(command.Parameters, "@polyline", entities.Select(e => e.Polyline));
            DbParameters.AddParameters(command.Parameters, "@expiry_date", entities.Select(e => e.ExpiryDate));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            if (keys.Count == 0) return;

            DbCommand cmd = SqlQuery.Delete().From("trains").WhereIn("id", keys.Count).ToPreparedCommand(connection);
            DbParameters.AddParameters(cmd.Parameters, "@id", keys);
            cmd.ExecuteNonQuery();
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
            DbParameters.AddParameters(cmd.Parameters, "@id", keyArray);
            return cmd.ExecuteReader();
        }

        private DbCommand selectAllCmd;
        protected override DbDataReader SelectAll()
        {
            selectAllCmd = selectAllCmd ?? baseQuery.ToPreparedCommand(connection);
            return selectAllCmd.ExecuteReader();
        }
    }
}
