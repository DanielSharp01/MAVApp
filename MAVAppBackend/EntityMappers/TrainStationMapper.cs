using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MAVAppBackend.Entities;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class TrainStationMapper : EntityMapper<int, TrainStation>
    {
        public class TrainIDSelector : MultiSelector<int, int, TrainStation, OrdinalEntityCollection<int, int, TrainStation>>
        {
            private readonly DatabaseConnection connection;
            private readonly SelectQuery baseQuery;
            private readonly Func<DbDataReader> selectAll;

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

            protected override int GetKey(DbDataReader reader)
            {
                return reader.GetInt32("train_id");
            }

            protected override int GetEntityKey(DbDataReader reader)
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

        public class MaximumOrdinalSelector : ArbitrarySelector<int, KeyedValue<int, int>>
        {
            private readonly DatabaseConnection connection;
            private readonly SelectQuery baseQuery;

            public MaximumOrdinalSelector(DatabaseConnection connection)
            {
                this.connection = connection;
                baseQuery = SqlQuery.Select().Column("train_id").Column("MAX(ordinal)", false, "ordinal").From("train_stations").GroupBy("train_id");
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

            private DatabaseCommand selectAllCmd;
            protected override DbDataReader SelectAll()
            {
                selectAllCmd = selectAllCmd ?? baseQuery.ToCommand(connection);
                return selectAllCmd.ExecuteReader();
            }

            protected override int GetKey(KeyedValue<int, int> value)
            {
                return value.Key;
            }

            protected override int GetKey(DbDataReader reader)
            {
                return reader.GetInt32("train_id");
            }

            protected override KeyedValue<int, int> CreateValue(int key)
            {
                return new KeyedValue<int, int>(key) {Value = -1};
            }

            protected override void Fill(KeyedValue<int, int> value, DbDataReader reader)
            {
                value.Value = reader.GetInt32("ordinal");
            }
        }

        public class InsertUniqueSelector : Selector<(int, int), int, TrainStation>
        {
            private readonly DatabaseConnection connection;
            private readonly SelectQuery baseQuery;
            private readonly Func<DbDataReader> selectAll;

            public InsertUniqueSelector(CacheContainer<int, TrainStation> cacheContainer, DatabaseConnection connection, SelectQuery baseQuery, Func<DbDataReader> selectAll)
                : base(cacheContainer, "unique")
            {
                this.connection = connection;
                this.baseQuery = baseQuery;
                this.selectAll = selectAll;
            }

            protected override TrainStation CreateEntity((int, int) key)
            {
                return new TrainStation() { TrainID = key.Item1, StationID = key.Item2};
            }

            protected override void CacheContainerOnUpdate(TrainStation entity)
            {
                cacheContainer.GetCache<(int, int)>("unique")[(entity.TrainID, entity.StationID)] = entity;
            }

            protected override (int, int) GetKey(TrainStation value)
            {
                return (value.TrainID, value.StationID);
            }

            protected override (int, int) GetKey(DbDataReader reader)
            {
                return (reader.GetInt32("train_id"), reader.GetInt32("station_id"));
            }

            protected override DbDataReader SelectAll()
            {
                return selectAll();
            }

            private DatabaseCommand selectByKeyCmd;
            protected override DbDataReader SelectByKey((int, int) key)
            {
                selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`train_id` = @train_id").Where("`station_id` = @station_id").ToPreparedCommand(connection);
                selectByKeyCmd.Parameters.Clear();
                selectByKeyCmd.Parameters.Add("@train_id", key.Item1);
                selectByKeyCmd.Parameters.Add("@station_id", key.Item2);
                return selectByKeyCmd.ExecuteReader();
            }

            protected override DbDataReader SelectByKeys(IList<(int, int)> keys)
            {
                DatabaseCommand cmd = baseQuery.Clone().WhereIn(new[] {"train_id", "station_id" }, keys.Count).ToPreparedCommand(connection);
                cmd.Parameters.AddMultiple("@train_id", keys.Select(key => key.Item1));
                cmd.Parameters.AddMultiple("@station_id", keys.Select(key => key.Item2));
                return cmd.ExecuteReader();
            }
        }

        private readonly SelectQuery baseQuery;

        public readonly TrainIDSelector ByTrainID;
        public readonly InsertUniqueSelector UniqueSelector;
        public readonly MaximumOrdinalSelector MaximumOrdinalByTrainID;

        public TrainStationMapper(DatabaseConnection connection)
            : base(connection)
        {
            baseQuery = SqlQuery.Select().AllColumns().From("train_stations");
            ByTrainID = new TrainIDSelector(cacheContainer, connection, baseQuery, SelectAll);
            UniqueSelector = new InsertUniqueSelector(cacheContainer, connection, baseQuery, SelectAll);
            MaximumOrdinalByTrainID = new MaximumOrdinalSelector(connection);
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

            Dictionary<int, KeyedValue<int, int>> maxOrdinals = new Dictionary<int, KeyedValue<int, int>>();
            MaximumOrdinalByTrainID.BeginSelect();
            foreach (var trainID in entities.Where(e => e.Ordinal == -1).Select(e => e.TrainID).Distinct())
            {
                maxOrdinals.Add(trainID, MaximumOrdinalByTrainID.GetByKey(trainID));
            }
            MaximumOrdinalByTrainID.EndSelect();

            foreach (var station in entities)
            {
                if (station.Ordinal == -1)
                    station.Ordinal = ++maxOrdinals[station.TrainID].Value;
            }

            command.Parameters.AddMultiple("@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            command.Parameters.AddMultiple("@train_id", entities.Select(e => e.TrainID));
            command.Parameters.AddMultiple("@ordinal", entities.Select(e => e.Ordinal));
            command.Parameters.AddMultiple("@station_id", entities.Select(e => e.StationID));
            command.Parameters.AddMultiple("@arrival", entities.Select(e => e.Arrival));
            command.Parameters.AddMultiple("@departure", entities.Select(e => e.Departure));
            command.Parameters.AddMultiple("@rel_distance", entities.Select(e => e.RelativeDistance));
            command.Parameters.AddMultiple("@platform", entities.Select(e => e.Platform));
            command.ExecuteNonQuery();

            UniqueSelector.BeginSelect();
            foreach (var entity in entities.Where(e => e.Key == -1))
            {
                UniqueSelector.FillByKey(entity);
            }
            UniqueSelector.EndSelect();
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
