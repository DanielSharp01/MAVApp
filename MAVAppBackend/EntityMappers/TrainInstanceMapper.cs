using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class TrainInstanceMapper : EntityMapper<long, TrainInstance>
    {
        private readonly SelectQuery baseQuery;

        public TrainInstanceMapper(DatabaseConnection connection)
            : base(connection)
        {
            baseQuery = SqlQuery.Select().From("train_instances").AllColumns();
        }

        private DatabaseCommand selectByKeyCmd;
        protected override DbDataReader SelectByKey(long key)
        {
            selectByKeyCmd = selectByKeyCmd ?? baseQuery.Where("`id` = @id").ToCommand(connection);
            selectAllCmd.Parameters.Add("@id", key);
            return selectByKeyCmd.ExecuteReader();
        }

        protected override DbDataReader SelectByKeys(IList<long> keys)
        {
            DatabaseCommand cmd = baseQuery.WhereIn("id", keys.Count).ToCommand(connection);
            cmd.Parameters.AddMultiple("@id", keys);
            return selectByKeyCmd.ExecuteReader();
        }

        private DatabaseCommand selectAllCmd;
        protected override DbDataReader SelectAll()
        {
            selectAllCmd = selectAllCmd ?? baseQuery.ToCommand(connection);
            return selectAllCmd.ExecuteReader();
        }

        protected override long GetKey(DbDataReader reader)
        {
            return reader.GetInt64("id");
        }

        protected override void InsertEntities(IList<TrainInstance> entities)
        {
            if (entities.Count == 0) return;

            DatabaseCommand command = SqlQuery.Insert().Columns(new[] { "id", "train_id", }).Into("train_instances").Values(entities.Count)
                .OnDuplicateKey("train_id").ToPreparedCommand(connection);

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key));
            command.Parameters.AddMultiple("@train_id", entities.Select(e => e.TrainID));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<long> keys)
        {
            if (keys.Count == 0) return;

            DatabaseCommand cmd = SqlQuery.Delete().From("train_instances").WhereIn("id", keys.Count).ToPreparedCommand(connection);
            cmd.Parameters.Add("@id", keys);
            cmd.ExecuteNonQuery();
        }
    }
}
