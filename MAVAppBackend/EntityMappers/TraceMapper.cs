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
        public class TrainInstanceSelector : MultiSelector<long, int, Trace, ListEntityCollection<long, int, Trace>>
        {
            private readonly DatabaseConnection connection;
            private readonly SelectQuery baseQuery;
            private readonly Func<DbDataReader> selectAll;

            public TrainInstanceSelector(CacheContainer<int, Trace> cacheContainer, DatabaseConnection connection, SelectQuery baseQuery, Func<DbDataReader> selectAll)
                : base(cacheContainer)
            {
                this.connection = connection;
                this.baseQuery = baseQuery;
                this.selectAll = selectAll;
            }

            protected override Trace CreateEntity(int key)
            {
                return new Trace() { Key = key };
            }

            protected override long GetCollectionKey(DbDataReader reader)
            {
                return reader.GetInt64("train_instance_id");
            }

            protected override int GetKey(DbDataReader reader)
            {
                return reader.GetInt32("id");
            }

            protected override DbDataReader SelectAll()
            {
                return selectAll();
            }

            private DatabaseCommand selectByKeyCmd;
            protected override DbDataReader SelectByKey(long key)
            {
                selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`train_instance_id` = @train_instance_id").OrderBy("updated", SelectQuery.ColumnOrder.Descending).ToPreparedCommand(connection);
                selectByKeyCmd.Parameters.Clear();
                selectByKeyCmd.Parameters.Add("@train_instance_id", key);
                return selectByKeyCmd.ExecuteReader();
            }

            protected override DbDataReader SelectByKeys(IList<long> keys)
            {
                DatabaseCommand cmd = baseQuery.Clone().WhereIn("train_instance_id", keys.Count).ToPreparedCommand(connection);
                cmd.Parameters.AddMultiple("@train_instance_id", keys);
                return cmd.ExecuteReader();
            }
        }

        private readonly SelectQuery baseQuery;

        public readonly TrainInstanceSelector ByTrainInstanceID;

        public TraceMapper(DatabaseConnection connection)
            : base(connection)
        {
            baseQuery = SqlQuery.Select().From("trace").AllColumns();
            ByTrainInstanceID = new TrainInstanceSelector(cacheContainer, connection, baseQuery, SelectAll);
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

            DatabaseCommand command = SqlQuery.Insert().Columns(new[] { "id", "train_instance_id", "lat", "lon" }).Into("trace").Values(entities.Count)
                .OnDuplicateKey(new[] { "train_instance_id", "lat", "lon"} ).ToPreparedCommand(connection);

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            command.Parameters.AddMultiple("@train_instance_id", entities.Select(e => e.TrainInstanceID));
            command.Parameters.AddMultipleVector2("@lat", "@lon", entities.Select(e => e.GPSCoord));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            if (keys.Count == 0) return;

            DatabaseCommand cmd = SqlQuery.Delete().From("trace").WhereIn("id", keys.Count).ToPreparedCommand(connection);
            cmd.Parameters.AddMultiple("@id", keys);
            cmd.ExecuteNonQuery();
        }
    }
}
