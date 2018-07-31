using MAVAppBackend.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MAVAppBackend.DataAccess;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class StationMapper : UpdatableEntityMapper<int, Station>
    {
        private readonly SelectQuery baseQuery;

        private readonly Dictionary<string, Station> normNameCache = new Dictionary<string, Station>();
        protected Dictionary<string, List<Station>> normNameSelectBatch;
        protected BatchSelectStrategy normNameBatchSelectStrategy;

        public StationMapper(DbConnection connection)
            : base(connection, new Dictionary<int, Station>())
        {
            baseQuery = SqlQuery.Select().AllColumns().From("stations");
        }

        protected override Station CreateEntity(int key)
        {
            return new Station(key);
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

        protected Station CreateEntityInternal(string normName)
        {
            if (entityCache == null)
            {
                return new Station(normName);
            }

            if (normNameCache.TryGetValue(normName, out Station entity))
                return entity;

            normNameCache.Add(normName, entity = new Station(normName));
            return entity;
        }

        protected override void FillEntity(Station entity, DbDataReader reader)
        {
            bool keyMissing = entity.Key == -1;
            base.FillEntity(entity, reader);

            if (keyMissing)
                CreateEntityInternal(entity.Key);
            else
                CreateEntityInternal(entity.NormalizedName);
        }

        public virtual Station GetByNormName(string normName, bool forceFill = true)
        {
            Station entity = CreateEntityInternal(normName);
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

                if (normNameCache.ContainsKey(entity.NormalizedName))
                    return;

                normNameCache.Add(entity.NormalizedName, new Station(entity.NormalizedName));
            }
            reader.Close();
        }

        private DbCommand selectByKeyCmd;
        protected override DbDataReader SelectByKey(int key)
        {
            selectByKeyCmd = selectByKeyCmd ?? baseQuery.Clone().Where("`id` = @id").ToPreparedCommand(connection);
            selectByKeyCmd.Parameters.Clear();
            DbParameters.AddParameter(selectByKeyCmd.Parameters, "@id", key);
            return selectByKeyCmd.ExecuteReader();
        }

        protected override DbDataReader SelectByKeys(IList<int> keys)
        {
            DbCommand cmd = baseQuery.Clone().WhereIn("id", keys.Count).ToPreparedCommand(connection);
            cmd.Parameters.Clear();
            DbParameters.AddParameters(cmd.Parameters, "@id", keys);
            return cmd.ExecuteReader();
        }

        private DbCommand selectAllCmd;
        protected override DbDataReader SelectAll()
        {
            selectAllCmd = selectAllCmd ?? baseQuery.ToCommand(connection);
            return selectAllCmd.ExecuteReader();
        }

        private DbCommand selectByNormNameCmd;
        private DbDataReader SelectByNormalizedName(string normName)
        {
            selectByNormNameCmd = selectByNormNameCmd ?? baseQuery.Clone().Where("`norm_name` = @norm_name").ToPreparedCommand(connection);
            selectByNormNameCmd.Parameters.Clear();
            DbParameters.AddParameter(selectByNormNameCmd.Parameters, "@norm_name", normName);
            return selectByNormNameCmd.ExecuteReader();
        }

        private DbDataReader SelectByNormalizedNames(IList<string> normNames)
        {
            DbCommand cmd = baseQuery.Clone().WhereIn("norm_name", normNames.Count).ToPreparedCommand(connection);
            DbParameters.AddParameters(cmd.Parameters, "@norm_name", normNames);
            return cmd.ExecuteReader();
        }

        protected override int GetKey(DbDataReader reader)
        {
            return reader.GetInt32("id");
        }

        protected override void InsertEntities(IList<Station> entities)
        {
            if (entities.Count == 0) return;

            DbCommand command = SqlQuery.Insert().Columns(new[] { "id", "name", "norm_name", "lat", "lon" }).Into("stations").Values(entities.Count).ToPreparedCommand(connection);

            DbParameters.AddParameters(command.Parameters, "@id", entities.Select(e => e.Key == -1 ? null : (object)e.Key));
            DbParameters.AddParameters(command.Parameters, "@name", entities.Select(e => e.Name));
            DbParameters.AddParameters(command.Parameters, "@norm_name", entities.Select(e => e.NormalizedName));
            DbParameterExtensions.AddVector2Parameters(command.Parameters, "@lat", "@lon", entities.Select(e => e.GPSCoord));
            command.ExecuteNonQuery();
        }

        public override void Update(Station entity)
        {
            if (entity.Key != -1) // We can't update only insert
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
