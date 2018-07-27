using System;
using System.Data.SqlClient;

namespace SharpEntities
{
    public abstract class UpdatableEntity<K> : Entity<K> where K : IEquatable<K>
    {
        public bool Changed { get; private set; }

        protected UpdatableEntity(K key)
            : base(key)
        { }

        public abstract void Fill(UpdatableEntity<K> other);

        public void OnSaved()
        {
            Changed = false;
        }

        public void OnChange()
        {
            Changed = true;
        }

    }
}