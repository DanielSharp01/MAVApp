using System.Data.Common;

namespace SharpEntities
{
    public abstract class Selector<B, K, E> : ArbitrarySelector<B, E> where E : Entity<K>, new()
    {
        protected CacheContainer<K, E> cacheContainer;
        protected string cacheName;

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
        public override E GetByKey(B key)
        {
            if (selectBatch != null && selectBatch.ContainsKey(key))
            {
                return selectBatch[key];
            }

            E entity = (cacheName != null ? cacheContainer.GetFromCache<B>(cacheName, key) : null) ?? CreateEntity(key);
            if (cacheName != null || !entity.Filled) FillByKey(entity);
            return entity;
        }


        public virtual E GetByKey(B key, bool forceFill)
        {
            if (selectBatch != null && selectBatch.ContainsKey(key))
            {
                return selectBatch[key];
            }

            E entity = (cacheName != null ? cacheContainer.GetFromCache<B>(cacheName, key) : null) ?? CreateEntity(key);
            if (cacheName != null || !entity.Filled || forceFill) FillByKey(entity);
            return entity;
        }

        protected abstract E CreateEntity(B key);

        protected abstract void CacheContainerOnUpdate(E entity);

        protected override E CreateValue(B key)
        {
            return CreateEntity(key);
        }

        protected override void Fill(E value, DbDataReader reader)
        {
            value.Fill(reader);
            value.Filled = true;
            cacheContainer.OnUpdate(value);
        }
    }
}
