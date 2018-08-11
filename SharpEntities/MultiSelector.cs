using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SharpEntities
{
    public abstract class MultiSelector<B, K, E, C> : ArbitrarySelector<B, C> where E : Entity<K>, new() where C : EntityCollection<B, K, E>, new()
    {
        protected CacheContainer<K, E> cacheContainer;

        protected MultiSelector(CacheContainer<K, E> cacheContainer)
        {
            this.cacheContainer = cacheContainer;
        }

        protected override void FillByKeySingle(C collection)
        {
            DbDataReader reader = SelectByKey(GetKey(collection));
            while (reader.Read())
            {
                Fill(collection, reader);
            }
            reader.Close();
        }

        protected abstract K GetEntityKey(DbDataReader reader);

        protected abstract E CreateEntity(K key);

        protected override C CreateValue(B key)
        {
            return new C() {Key = key};
        }

        protected override void Fill(C value, DbDataReader reader)
        {
            var entity = CreateEntity(GetEntityKey(reader));
            entity.Fill(reader);
            entity.Filled = true;
            value.Add(entity);
            value.Filled = true;
            cacheContainer.OnUpdate(entity);
        }

        protected override B GetKey(C value)
        {
            return value.Key;
        }
    }
}
