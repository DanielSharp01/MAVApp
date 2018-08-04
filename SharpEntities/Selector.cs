using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SharpEntities
{
    public abstract class Selector<B, K, E> where E : Entity<K>, new()
    {
        protected CacheContainer<K, E> cacheContainer;
        protected string cacheName = null;
        protected Dictionary<B, E> selectBatch;
        protected BatchSelectStrategy batchStrategy;

        protected Selector(CacheContainer<K, E> cacheContainer, string cacheName = null)
        {
            this.cacheContainer = cacheContainer;
            this.cacheName = cacheName;

            if (cacheName != null)
            {
                cacheContainer.RequestCache<B>(cacheName);
                cacheContainer.Update += CacheContainerOnUpdate;
            }
        }

        public virtual void BeginSelect(BatchSelectStrategy batchSelectStrategy = BatchSelectStrategy.MultiKey)
        {
            if (selectBatch != null) return;
            batchStrategy = batchSelectStrategy;
            selectBatch = new Dictionary<B, E>();
        }

        public virtual void EndSelect()
        {
            if (selectBatch == null) return;

            if (selectBatch.Count == 0)
            {
                selectBatch = null;
                return;
            }

            DbDataReader reader = (batchStrategy == BatchSelectStrategy.MultiKey) ? SelectByKeys(selectBatch.Keys.ToList()) : SelectAll();

            while (reader.Read())
            {
                B key = GetKey(reader);
                if (!selectBatch.ContainsKey(key)) continue;

                var entity = selectBatch[key];
                entity.Fill(reader);
                entity.Filled = true;
                cacheContainer.OnUpdate(entity);
            }

            reader.Close();
            selectBatch = null;
        }

        public virtual E GetByKey(B key, bool forceFill = true)
        {
            if (selectBatch != null && selectBatch.ContainsKey(key))
            {
                return selectBatch[key];
            }

            E entity = (cacheName != null ? cacheContainer.GetFromCache<B>(cacheName, key) : null) ?? CreateEntity(key);
            if (cacheName != null || !entity.Filled || forceFill) FillByKey(entity);
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
                B key = GetKey(entity);
                if (!selectBatch.ContainsKey(key))
                    selectBatch[key] = entity;
            }
        }

        protected virtual void FillByKeySingle(E entity)
        {
            DbDataReader reader = SelectByKey(GetKey(entity));
            if (reader.Read())
            {
                entity.Fill(reader);
                entity.Filled = true;
                cacheContainer.OnUpdate(entity);
            }
            reader.Close();
        }

        protected abstract DbDataReader SelectByKey(B key);

        protected abstract DbDataReader SelectByKeys(IList<B> keys);

        protected abstract DbDataReader SelectAll();

        protected abstract B GetKey(E entity);

        protected abstract B GetKey(DbDataReader reader);

        protected abstract E CreateEntity(B key);

        protected abstract void CacheContainerOnUpdate(E entity);
    }
}
