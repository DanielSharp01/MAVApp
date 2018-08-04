using System;
using System.Collections.Generic;
using System.Text;

namespace SharpEntities
{
    public delegate void UpdateEntityCacheEventHandler<K, E>(E entity) where E : Entity<K>;
    public class CacheContainer<K, E> where E : Entity<K>
    {
        Dictionary<string, object> selectorCaches = new Dictionary<string, object>();

        public event UpdateEntityCacheEventHandler<K, E> Update;

        public void RequestCache<B>(string name)
        {
            selectorCaches.Add(name, new Dictionary<B, E>());
        }

        public void OnUpdate(E entity)
        {
            Update?.Invoke(entity);
        }

        public Dictionary<B, E> GetCache<B>(string name)
        {
            return (Dictionary<B, E>)selectorCaches[name];
        }

        public E GetFromCache<B>(string name, B key)
        {
            Dictionary<B, E> dict = (Dictionary<B, E>)selectorCaches[name];
            return dict.ContainsKey(key) ? dict[key] : null;
        }
    }
}
