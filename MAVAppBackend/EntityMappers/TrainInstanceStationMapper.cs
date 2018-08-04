using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.Entities;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class TrainInstanceStationMapper : EntityMapper<int, TrainInstanceStation>
    {
        private readonly SelectQuery baseQuery;

        public TrainInstanceStationMapper(DatabaseConnection connection)
            : base(connection, new Dictionary<int, TrainInstanceStation>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("train_instance_stations");
        }

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }

        protected override void InsertEntities(IList<TrainInstanceStation> entities)
        {
            if (entities.Count == 0) return;

            DatabaseCommand command = SqlQuery.Insert().Columns(new[] { "id", "train_instance_id", "train_station_id", "actual_arrival", "actual_departure" }).Into("train_instance_stations").Values(entities.Count)
                .OnDuplicateKey(new[] { "train_instance_id", "train_station_id", "actual_arrival", "actual_departure" }).ToPreparedCommand(connection);

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key));
            command.Parameters.AddMultiple("@train_instance_id", entities.Select(e => e.TrainInstanceID));
            command.Parameters.AddMultiple("@train_station_id", entities.Select(e => e.TrainStationID));
            command.Parameters.AddMultiple("@actual_arrival", entities.Select(e => e.ActualArrival));
            command.Parameters.AddMultiple("@actual_departure", entities.Select(e => e.ActualDeparture));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            if (keys.Count == 0) return;

            DatabaseCommand cmd = SqlQuery.Delete().From("train_instance_stations").WhereIn("id", keys.Count).ToPreparedCommand(connection);
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
