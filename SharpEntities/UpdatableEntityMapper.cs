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

        public void BeginUpdate()
        {
            if (insertables != null) return;
            insertables = new List<E>();
        }

        public void EndUpdate()
        {
            if (insertables == null) return;
            InsertEntities(insertables);
        }

        public void UpdateSaveCache()
        {
            if (entityCache == null) throw new InvalidOperationException("Cache is not set up and thus cannot be saved.");

            foreach (KeyValuePair<K, E> kvp in entityCache)
            {
                if (kvp.Value.Changed)
                    Update(kvp.Value);
            }
        }

        public void Update(E entity)
        {
            if (insertables != null)
                insertables.Add(entity);
            else
                InsertEntities(new[] { entity });

            entity.OnSaved();
        }

        protected abstract void InsertEntities(IEnumerable<E> entities);
    }
}
