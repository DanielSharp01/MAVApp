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
        private readonly SelectQuery baseQuery;

        private readonly Dictionary<string, Station> normNameCache = new Dictionary<string, Station>();
        protected Dictionary<string, List<Station>> normNameSelectBatch;
        protected BatchSelectStrategy normNameBatchSelectStrategy;

        public StationMapper(DatabaseConnection connection)
            : base(connection, new Dictionary<int, Station>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("stations");
        }

        public virtual void BeginSelectNormName(BatchSelectStrategy batchSelectStrategy = BatchSelectStrategy.MultiKey)
        {
            if (selectBatch != null) return;
            normNameBatchSelectStrategy = batchSelectStrategy;
            normNameSelectBatch = new Dictionary<string, List<Station>>();
        }

        public virtual void EndSelectNormName()
        {
            if (normNameSelectBatch == null) return;

            if (normNameSelectBatch.Count == 0)
            {
                normNameSelectBatch = null;
                return;
            }

            DbDataReader reader = (normNameBatchSelectStrategy == BatchSelectStrategy.MultiKey) ? SelectByNormalizedNames(normNameSelectBatch.Keys.ToList()) : SelectAll();

            if (reader.Read())
            {
                do
                {
                    string key = reader.GetString("norm_name");
                    if (!normNameSelectBatch.ContainsKey(key)) continue;

                    foreach (Station entity in normNameSelectBatch[key])
                    {
                        FillEntity(entity, reader);
                    }
                } while (AdvanceReader(reader));
            }

            reader.Close();
            normNameSelectBatch = null;
        }

        protected Station CreateEntity(string normName)
        {
            if (entityCache == null)
            {
                return new Station() {NormalizedName=normName};
            }

            if (normNameCache.TryGetValue(normName, out Station entity))
                return entity;

            normNameCache.Add(normName, entity = new Station() { NormalizedName = normName });
            return entity;
        }

        protected override void FillEntity(Station entity, DbDataReader reader)
        {
            bool keyMissing = entity.Key == -1;
            base.FillEntity(entity, reader);

            if (keyMissing)
                CreateEntity(entity.Key);
            else
                CreateEntity(entity.NormalizedName);
        }

        public virtual Station GetByNormName(string normName, bool forceFill = true)
        {
            Station entity = CreateEntity(normName);
            if (normNameCache == null || forceFill) FillByNormName(entity);
            return entity;
        }

        public virtual void FillByNormName(Station entity)
        {
            if (selectBatch == null)
            {
                FillByNormNameSingle(entity);
            }
            else
            {
                if (!normNameSelectBatch.ContainsKey(entity.NormalizedName))
                    normNameSelectBatch.Add(entity.NormalizedName, new List<Station>());

                normNameSelectBatch[entity.NormalizedName].Add(entity);
            }
        }

        protected virtual void FillByNormNameSingle(Station entity)
        {
            DbDataReader reader = SelectByNormalizedName(entity.NormalizedName);
            if (reader.Read())
            {
                FillEntity(entity, reader);
            }
            reader.Close();
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
            cmd.Parameters.Add("@id", keys);
            return cmd.ExecuteReader();
        }

        private DatabaseCommand selectAllCmd;
        protected override DbDataReader SelectAll()
        {
            selectAllCmd = selectAllCmd ?? baseQuery.ToCommand(connection);
            return selectAllCmd.ExecuteReader();
        }

        private DatabaseCommand selectByNormNameCmd;
        private DbDataReader SelectByNormalizedName(string normName)
        {
            selectByNormNameCmd = selectByNormNameCmd ?? baseQuery.Clone().Where("`norm_name` = @norm_name").ToPreparedCommand(connection);
            selectByNormNameCmd.Parameters.Clear();
            selectByNormNameCmd.Parameters.Add("@norm_name", normName);
            return selectByNormNameCmd.ExecuteReader();
        }

        private DbDataReader SelectByNormalizedNames(IList<string> normNames)
        {
            DatabaseCommand cmd = baseQuery.Clone().WhereIn("norm_name", normNames.Count).ToPreparedCommand(connection);
            cmd.Parameters.Add("@norm_name", normNames);
            return cmd.ExecuteReader();
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
