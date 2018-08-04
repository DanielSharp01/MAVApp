using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MAVAppBackend.DataAccess;
using MAVAppBackend.Entities;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class StationMapper : EntityMapper<int, Station>
    {
        public class NormalizedNameSelector : Selector<string, int, Station>
        {
            private readonly DatabaseConnection connection;
            private readonly SelectQuery baseQuery;
            private readonly Func<DbDataReader> selectAll;

            private DbDataReader SelectByNormalizedNames(IList<string> normNames)
            {
                DatabaseCommand cmd = baseQuery.Clone().WhereIn("norm_name", normNames.Count).ToPreparedCommand(connection);
                cmd.Parameters.Add("@norm_name", normNames);
                return cmd.ExecuteReader();
            }

            public NormalizedNameSelector(CacheContainer<int, Station> cacheContainer, Func<DbDataReader> selectAll, DatabaseConnection connection, SelectQuery baseQuery)
                : base(cacheContainer, "norm_name")
            {
                this.connection = connection;
                this.baseQuery = baseQuery;
                this.selectAll = selectAll;
            }

            private DatabaseCommand selectCmd;
            protected override DbDataReader SelectByKey(string key)
            {
                selectCmd = selectCmd ?? baseQuery.Clone().Where("`norm_name` = @norm_name").ToPreparedCommand(connection);
                selectCmd.Parameters.Clear();
                selectCmd.Parameters.Add("@norm_name", key);
                return selectCmd.ExecuteReader();
            }

            protected override DbDataReader SelectByKeys(IList<string> keys)
            {
                DatabaseCommand cmd = baseQuery.Clone().WhereIn("norm_name", keys.Count).ToPreparedCommand(connection);
                cmd.Parameters.AddMultiple("@norm_name", keys);
                return cmd.ExecuteReader();
            }

            protected override DbDataReader SelectAll()
            {
                return selectAll();
            }

            protected override string GetKey(Station entity)
            {
                return entity.NormalizedName;
            }

            protected override string GetKey(DbDataReader reader)
            {
                return reader.GetString("norm_name");
            }

            protected override Station CreateEntity(string key)
            {
                return new Station() { NormalizedName = key };
            }

            protected override void CacheContainerOnUpdate(Station entity)
            {
                cacheContainer.GetCache<string>("norm_name")[entity.NormalizedName] = entity;
            }
        }

        private readonly SelectQuery baseQuery;
        public readonly NormalizedNameSelector ByNormName;

        public StationMapper(DatabaseConnection connection)
            : base(connection)
        {
            baseQuery = SqlQuery.Select().AllColumns().From("stations");
            ByNormName = new NormalizedNameSelector(cacheContainer, SelectAll, connection, baseQuery);
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
            cmd.Parameters.Clear();
            cmd.Parameters.AddMultiple("@id", keys);
            return cmd.ExecuteReader();
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

        protected override void InsertEntities(IList<Station> entities)
        {
            if (entities.Count == 0) return;

            DatabaseCommand command = SqlQuery.Insert().Columns(new[] { "name", "norm_name", "lat", "lon" }).Into("stations").Values(entities.Count).ToPreparedCommand(connection);
            
            command.Parameters.AddMultiple("@name", entities.Select(e => e.Name));
            command.Parameters.AddMultiple("@norm_name", entities.Select(e => e.NormalizedName));
            command.Parameters.AddMultipleVector2("@lat", "@lon", entities.Select(e => e.GPSCoord));
            command.ExecuteNonQuery();
        }

        public override void Update(Station entity)
        {
            if (entity.Key != -1) // Don't update only insert
                return;

            base.Update(entity);
        }

        public override void UpdateSaveCache()
        {
            // Don't update only insert
        }

        public override void BeginDelete()
        {
            throw new NotImplementedException();
        }

        public override void EndDelete()
        {
            throw new NotImplementedException();
        }

        public override void Delete(int key)
        {
            throw new NotImplementedException();
        }

        public override void Delete(Station entity)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteEntities(IList<int> keys)
        {
            throw new NotImplementedException();
        }
    }
}
