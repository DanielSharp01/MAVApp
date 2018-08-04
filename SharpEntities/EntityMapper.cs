using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace SharpEntities
{
    public enum BatchSelectStrategy
    {
        MultiKey,
        All
    }

    public abstract class EntityMapper<K, E> where E : Entity<K>, new()
    {
        protected DatabaseConnection connection;
        
        protected Dictionary<K, List<E>> selectBatch;
        protected BatchSelectStrategy batchSelectStrategy;

        protected IDictionary<K, E> entityCache;

        public EntityMapper(DatabaseConnection connection, IDictionary<K, E> entityCache = null)
        {
            this.connection = connection;
            this.entityCache = entityCache;
        }

        public virtual void BeginSelect(BatchSelectStrategy batchSelectStrategy = BatchSelectStrategy.MultiKey)
        {
            if (selectBatch != null) return;
            this.batchSelectStrategy = batchSelectStrategy;
            selectBatch = new Dictionary<K, List<E>>();
        }

        public virtual void EndSelect()
        {
            if (selectBatch == null) return;

            if (selectBatch.Count == 0)
            {
                selectBatch = null;
                return;
            }

            DbDataReader reader = (batchSelectStrategy == BatchSelectStrategy.MultiKey) ? SelectByKeys(selectBatch.Keys.ToList()) : SelectAll();

            if (reader.Read())
            {
                do
                {
                    K key = GetKey(reader);
                    if (!selectBatch.ContainsKey(key)) continue;

                    foreach (E entity in selectBatch[key])
                    {
                        FillEntity(entity, reader);
                    }
                } while (AdvanceReader(reader));
            }

            reader.Close();
            selectBatch = null;
        }

        protected virtual E CreateEntity(K key)
        {
            if (entityCache == null)
            {
                return new E() {Key = key};
            }

            if (entityCache.TryGetValue(key, out E entity))
                return entity;

            entityCache.Add(key, entity = new E() { Key = key };
            return entity;
        }

        public virtual E GetByKey(K key, bool forceFill = true)
        {
            E entity = CreateEntity(key);
            if (entityCache == null || forceFill) FillByKey(entity);
            return entity;
        }

        public virtual void FillByKey(E entity)
        {
            if (selectBatch == null)
            {
                FillByKeySingle(entity);
            }
            else
            {
                if (!selectBatch.ContainsKey(entity.Key))
                    selectBatch.Add(entity.Key, new List<E>());

                selectBatch[entity.Key].Add(entity);
            }
        }

        protected virtual void FillByKeySingle(E entity)
        {
            DbDataReader reader = SelectByKey(entity.Key);
            if (reader.Read())
            {
                FillEntity(entity, reader);
            }
            reader.Close();
        }

        public virtual List<E> GetAll()
        {
            List<E> entities = new List<E>();

            DbDataReader reader = SelectAll();
            if (reader.Read())
            {
                do 
                {
                    E entity = CreateEntity(GetKey(reader));
                    FillEntity(entity, reader);
                    entities.Add(entity);
                } while (AdvanceReader(reader));
            }

            reader.Close();

            return entities;
        }

        protected virtual void FillEntity(E entity, DbDataReader reader)
        {
            entity.Fill(reader);
        }

        protected abstract DbDataReader SelectByKey(K key);

        protected abstract DbDataReader SelectByKeys(IList<K> keys);

        protected abstract DbDataReader SelectAll();

        protected abstract K GetKey(DbDataReader reader);

        protected virtual bool AdvanceReader(DbDataReader reader)
        {
            return reader.Read();
        }

        private List<E> insertables = null;
        private List<K> deletables = null;

        public virtual void BeginUpdate()
        {
            if (insertables != null) return;
            insertables = new List<E>();
        }

        public virtual void EndUpdate()
        {
            if (insertables == null) return;
            InsertEntities(insertables);
        }

        public virtual void BeginDelete()
        {
            if (deletables != null) return;
            deletables = new List<K>();
        }


        public virtual void EndDelete()
        {
            if (deletables == null) return;
            DeleteEntities(deletables);
        }

        public virtual void UpdateSaveCache()
        {
            foreach (KeyValuePair<K, E> kvp in entityCache)
            {
                if (kvp.Value.Changed)
                    Update(kvp.Value);
            }
        }

        public virtual void Update(E entity)
        {
            if (insertables != null)
                insertables.Add(entity);
            else
                InsertEntities(new[] { entity });

            entity.OnSaved();
        }

        public virtual void Delete(E entity)
        {
            if (deletables != null)
                deletables.Add(entity.Key);
            else
                DeleteEntities(new[] { entity.Key });
        }

        public virtual void Delete(K key)
        {
            if (deletables != null)
                deletables.Add(key);
            else
                DeleteEntities(new[] { key });
        }

        protected abstract void InsertEntities(IList<E> entities);
        protected abstract void DeleteEntities(IList<K> keys);
    }
}
