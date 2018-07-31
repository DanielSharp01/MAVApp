using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SharpEntities
{
    public abstract class UpdatableEntityMapper<K, E> : EntityMapper<K, E> where E : UpdatableEntity<K> where K : IEquatable<K>
    {
        public UpdatableEntityMapper(DbConnection connection, IDictionary<K, E> entityCache = null)
            : base(connection, entityCache)
        { }

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
            if (entityCache == null) throw new InvalidOperationException("Cache is not set up and thus cannot be saved.");

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
