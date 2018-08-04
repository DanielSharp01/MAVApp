using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SharpEntities
{
    public abstract class MultiSelector<B, K, E, C> where E : Entity<K>, new() where C : EntityCollection<B, K, E>, new()
    {
        protected CacheContainer<K, E> cacheContainer;
        protected Dictionary<B, C> selectBatch;
        protected BatchSelectStrategy batchStrategy;

        protected MultiSelector(CacheContainer<K, E> cacheContainer)
        {
            this.cacheContainer = cacheContainer;
        }

        public virtual void BeginSelect(BatchSelectStrategy batchSelectStrategy = BatchSelectStrategy.MultiKey)
        {
            if (selectBatch != null) return;
            batchStrategy = batchSelectStrategy;
            selectBatch = new Dictionary<B, C>();
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
                B collectionKey = GetCollectionKey(reader);
                K key = GetKey(reader);
                if (!selectBatch.ContainsKey(collectionKey)) continue;

                var entityCollection = selectBatch[collectionKey];
                var entity = CreateEntity(key);
                entity.Fill(reader);
                entity.Filled = true;
                entityCollection.Add(entity);
                cacheContainer.OnUpdate(entity);
            }

            foreach (var entityCollection in selectBatch.Values)
            {
                entityCollection.Filled = true;
            }

            reader.Close();
            selectBatch = null;
        }

        public virtual C GetByKey(B key)
        {
            if (selectBatch != null && selectBatch.ContainsKey(key))
            {
                return selectBatch[key];
            }

            C entityCollection = new C() {Key = key};
            FillByKey(entityCollection);
            return entityCollection;
        }

        public virtual void FillByKey(C entityCollection)
        {
            if (selectBatch == null)
            {
                FillByKeySingle(entityCollection);
            }
            else
            {
                if (!selectBatch.ContainsKey(entityCollection.Key))
                    selectBatch[entityCollection.Key] = entityCollection;
            }
        }

        protected virtual void FillByKeySingle(C collection)
        {
            DbDataReader reader = SelectByKey(collection.Key);
            while (reader.Read())
            {
                var entity = CreateEntity(GetKey(reader));
                entity.Fill(reader);
                entity.Filled = true;
                collection.Add(entity);
                collection.Filled = true;
                cacheContainer.OnUpdate(entity);
            }
            reader.Close();
        }

        protected abstract DbDataReader SelectByKey(B key);

        protected abstract DbDataReader SelectByKeys(IList<B> keys);

        protected abstract DbDataReader SelectAll();

        protected abstract B GetCollectionKey(DbDataReader reader);

        protected abstract K GetKey(DbDataReader reader);

        protected abstract E CreateEntity(K key);
    }
}
