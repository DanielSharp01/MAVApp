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
        public class InsertUniqueSelector : Selector<(long, int), int, TrainInstanceStation>
        {
            private readonly DatabaseConnection connection;
            private readonly SelectQuery baseQuery;
            private readonly Func<DbDataReader> selectAll;

            public InsertUniqueSelector(CacheContainer<int, TrainInstanceStation> cacheContainer, DatabaseConnection connection, SelectQuery baseQuery, Func<DbDataReader> selectAll)
                : base(cacheContainer, "unique")
            {
                this.connection = connection;
                this.baseQuery = baseQuery;
                this.selectAll = selectAll;
            }

            protected override TrainInstanceStation CreateEntity((long, int) key)
            {
                return new TrainInstanceStation() { TrainInstanceID = key.Item1, TrainStationID = key.Item2 };
            }

            protected override void CacheContainerOnUpdate(TrainInstanceStation entity)
            {
                cacheContainer.GetCache<(long, int)>("unique")[(entity.TrainInstanceID, entity.TrainStationID)] = entity;
            }

            protected override (long, int) GetKey(TrainInstanceStation value)
            {
                return (value.TrainInstanceID, value.TrainStationID);
            }

            protected override (long, int) GetKey(DbDataReader reader)
            {
                return (reader.GetInt32("train_instance_id"), reader.GetInt32("train_station_id"));
            }

            protected override DbDataReader SelectAll()
            {
                return selectAll();
            }

            private DatabaseCommand selectByKeyCmd;
            protected override DbDataReader SelectByKey((long, int) key)
            {
                selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`train_instance_id` = @train_instance_id").Where("`train_station_id` = @train_station_id").ToPreparedCommand(connection);
                selectByKeyCmd.Parameters.Clear();
                selectByKeyCmd.Parameters.Add("@train_instance_id", key.Item1);
                selectByKeyCmd.Parameters.Add("@train_station_id", key.Item2);
                return selectByKeyCmd.ExecuteReader();
            }

            protected override DbDataReader SelectByKeys(IList<(long, int)> keys)
            {
                DatabaseCommand cmd = baseQuery.Clone().WhereIn(new[] { "train_instance_id", "train_station_id" }, keys.Count).ToPreparedCommand(connection);
                cmd.Parameters.AddMultiple("@train_instance_id", keys.Select(key => key.Item1));
                cmd.Parameters.AddMultiple("@train_station_id", keys.Select(key => key.Item2));
                return cmd.ExecuteReader();
            }
        }

        private readonly SelectQuery baseQuery;

        protected readonly InsertUniqueSelector uniqueSelector;

        public TrainInstanceStationMapper(DatabaseConnection connection)
            : base(connection)
        {
            baseQuery = SqlQuery.Select().AllColumns().From("train_instance_stations");
            uniqueSelector = new InsertUniqueSelector(cacheContainer, connection, baseQuery, SelectAll);
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

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            command.Parameters.AddMultiple("@train_instance_id", entities.Select(e => e.TrainInstanceID));
            command.Parameters.AddMultiple("@train_station_id", entities.Select(e => e.TrainStationID));
            command.Parameters.AddMultiple("@actual_arrival", entities.Select(e => e.ActualArrival));
            command.Parameters.AddMultiple("@actual_departure", entities.Select(e => e.ActualDeparture));
            command.ExecuteNonQuery();

            uniqueSelector.BeginSelect();
            foreach (var entity in entities.Where(e => e.Key == -1))
            {
                uniqueSelector.FillByKey(entity);
            }
            uniqueSelector.EndSelect();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            if (keys.Count == 0) return;

            DatabaseCommand cmd = SqlQuery.Delete().From("train_instance_stations").WhereIn("id", keys.Count).ToPreparedCommand(connection);
            cmd.Parameters.AddMultiple("@id", keys);
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
            cmd.Parameters.AddMultiple("@id", keys);
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
