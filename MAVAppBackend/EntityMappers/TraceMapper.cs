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
    public class TraceMapper : EntityMapper<int, Trace>
    {
        private readonly SelectQuery baseQuery;

        public TraceMapper(DatabaseConnection connection)
            : base(connection, new Dictionary<int, Trace>())
        {
            baseQuery = SqlQuery.Select().From("trace").AllColumns();
        }

        private DatabaseCommand selectByKeyCmd;
        protected override DbDataReader SelectByKey(int key)
        {
            selectByKeyCmd = selectByKeyCmd ?? baseQuery.Where("`id` = @id").ToCommand(connection);
            selectAllCmd.Parameters.Add("@id", key);
            return selectByKeyCmd.ExecuteReader();
        }

        protected override DbDataReader SelectByKeys(IList<int> keys)
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

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }

        protected override void InsertEntities(IList<Trace> entities)
        {
            if (entities.Count == 0) return;

            DatabaseCommand command = SqlQuery.Insert().Columns(new[] { "id", "train_instance_id", "lat", "lon", "updated" }).Into("trace").Values(entities.Count)
                .OnDuplicateKey(new[] { "train_instance_id", "lat", "lon", "updated"} ).ToPreparedCommand(connection);

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            command.Parameters.AddMultiple("@train_instance_id", entities.Select(e => e.TrainInstanceID));
            command.Parameters.AddMultipleVector2("@lat", "@lon", entities.Select(e => e.GPSCoord));
            command.Parameters.AddMultiple("@updated", entities.Select(e => e.Updated));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            if (keys.Count == 0) return;

            DatabaseCommand cmd = SqlQuery.Delete().From("trace").WhereIn("id", keys.Count).ToPreparedCommand(connection);
            cmd.Parameters.Add("@id", keys);
            cmd.ExecuteNonQuery();
        }
    }
}
