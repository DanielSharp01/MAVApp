using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class TrainMapper : EntityMapper<int, Train>
    {
        private readonly SelectQuery baseQuery;

        public TrainMapper(DatabaseConnection connection)
            : base(connection)
        {
            baseQuery = SqlQuery.Select().AllColumns().From("trains");
        }

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }

        protected override void InsertEntities(IList<Train> entities)
        {
            if (entities.Count == 0) return;

            DatabaseCommand command = SqlQuery.Insert().Columns(new[] {"id", "name", "type", "polyline", "expiry_date"}).Into("trains").Values(entities.Count)
                .OnDuplicateKey(new[] {"name", "type", "polyline", "expiry_date"}).ToPreparedCommand(connection);

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            command.Parameters.AddMultiple("@name", entities.Select(e => e.Name));
            command.Parameters.AddMultiple("@type", entities.Select(e => e.Type));
            command.Parameters.AddMultiplePolyline("@polyline", entities.Select(e => e.Polyline));
            command.Parameters.AddMultiple("@expiry_date", entities.Select(e => e.ExpiryDate));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            if (keys.Count == 0) return;

            DatabaseCommand cmd = SqlQuery.Delete().From("trains").WhereIn("id", keys.Count).ToPreparedCommand(connection);
            cmd.Parameters.Add("@id", keys);
            cmd.ExecuteNonQuery();
        }

        private DatabaseCommand selectByKeyCmd;
        protected override DbDataReader SelectByKey(int key)
        {
            selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`id` = @id").ToPreparedCommand(connection);
            selectByKeyCmd.Parameters.Clear();
            selectByKeyCmd.Parameters.Add("@id", key);
            return selectByKeyCmd.ExecuteReader();
        }

        protected override DbDataReader SelectByKeys(IList<int> keys)
        {
            DatabaseCommand cmd = baseQuery.Clone().WhereIn("id", keys.Count).ToPreparedCommand(connection);
            cmd.Parameters.Add("@id", keys);
            return cmd.ExecuteReader();
        }

        private DatabaseCommand selectAllCmd;
        protected override DbDataReader SelectAll()
        {
            selectAllCmd = selectAllCmd ?? baseQuery.ToPreparedCommand(connection);
            return selectAllCmd.ExecuteReader();
        }
    }
}
