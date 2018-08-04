using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class TrainStationMapper : EntityMapper<int, TrainStation>
    {
        public class TrainIDSelector : MultiSelector<int, int, TrainStation, OrdinalEntityCollection<int, int, TrainStation>>
        {
            private DatabaseConnection connection;
            private SelectQuery baseQuery;
            private Func<DbDataReader> selectAll;

            public TrainIDSelector(CacheContainer<int, TrainStation> cacheContainer, DatabaseConnection connection, SelectQuery baseQuery, Func<DbDataReader> selectAll)
                : base(cacheContainer)
            {
                this.connection = connection;
                this.baseQuery = baseQuery;
                this.selectAll = selectAll;
            }

            protected override TrainStation CreateEntity(int key)
            {
                return new TrainStation() {Key = key};
            }

            protected override int GetCollectionKey(DbDataReader reader)
            {
                return reader.GetInt32("train_id");
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
            protected override DbDataReader SelectByKey(int key)
            {
                selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`train_id` = @train_id").ToPreparedCommand(connection);
                selectByKeyCmd.Parameters.Clear();
                selectByKeyCmd.Parameters.Add("@train_id", key);
                return selectByKeyCmd.ExecuteReader();
            }

            protected override DbDataReader SelectByKeys(IList<int> keys)
            {
                DatabaseCommand cmd = baseQuery.Clone().WhereIn("train_id", keys.Count).ToPreparedCommand(connection);
                cmd.Parameters.AddMultiple("@train_id", keys);
                return cmd.ExecuteReader();
            }
        }

        private readonly SelectQuery baseQuery;

        public readonly TrainIDSelector ByTrainID;

        public TrainStationMapper(DatabaseConnection connection)
            : base(connection)
        {
            baseQuery = SqlQuery.Select().AllColumns().From("train_stations");
            ByTrainID = new TrainIDSelector(cacheContainer, connection, baseQuery, SelectAll);
        }

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }

        protected override void InsertEntities(IList<TrainStation> entities)
        {
            if (entities.Count == 0) return;

            DatabaseCommand command = SqlQuery.Insert().Columns(new[] { "id", "train_id", "ordinal", "station_id", "arrival", "departure", "rel_distance", "platform" }).Into("train_stations").Values(entities.Count)
                .OnDuplicateKey(new[] { "train_id", "ordinal", "station_id", "arrival", "departure", "rel_distance", "platform" }).ToPreparedCommand(connection);

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            command.Parameters.AddMultiple("@train_id", entities.Select(e => e.TrainID));
            command.Parameters.AddMultiple("@ordinal", entities.Select(e => e.Ordinal));
            command.Parameters.AddMultiple("@station_id", entities.Select(e => e.StationID));
            command.Parameters.AddMultiple("@arrival", entities.Select(e => e.Arrival));
            command.Parameters.AddMultiple("@departure", entities.Select(e => e.Departure));
            command.Parameters.AddMultiple("@rel_distance", entities.Select(e => e.RelativeDistance));
            command.Parameters.AddMultiple("@platform", entities.Select(e => e.Platform));
            command.ExecuteNonQuery();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            if (keys.Count == 0) return;

            DatabaseCommand cmd = SqlQuery.Delete().From("train_stations").WhereIn("id", keys.Count).ToPreparedCommand(connection);
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
