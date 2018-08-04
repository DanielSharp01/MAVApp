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

    public abstract class EntityMapper<K, E> : Selector<K, K, E> where E : Entity<K>, new()
    {
        protected DatabaseConnection connection;

        protected EntityMapper(DatabaseConnection connection)
            : base(new CacheContainer<K, E>(), "key")
        {
            this.connection = connection;
        }

        protected override E CreateEntity(K key)
        {
            return new E() { Key = key };
        }

        protected override K GetKey(E entity)
        {
            return entity.Key;
        }

        public List<E> GetAll()
        {
            List<E> entities = new List<E>();
            DbDataReader reader = SelectAll();
            while (reader.Read())
            {
                var entity = new E() { Key = GetKey(reader) };
                entity.Fill(reader);
                entity.Filled = true;
                cacheContainer.OnUpdate(entity);
                entities.Add(entity);
            }

            return entities;
        }

        protected override void CacheContainerOnUpdate(E entity)
        {
            cacheContainer.GetCache<K>("key")[entity.Key] = entity;
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
            foreach (KeyValuePair<K, E> kvp in cacheContainer.GetCache<K>("key"))
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
