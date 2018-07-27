using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace SharpEntities
{
    /// <summary>
    /// Represents an abstract entity in the database which has an Key
    /// </summary>
    public abstract class Entity<K> where K : IEquatable<K>
    {
        /// <summary>
        /// Database Key
        /// </summary>
        public K Key { get; protected set; }

        /// <summary>
        /// Tells whether this object was filled from the database or not
        /// </summary>
        public bool Filled { protected set; get; } = false;

        /// <param name="key">Database Key</param>
        public Entity(K key)
        {
            Key = key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (!(obj is Entity<K> entity)) return false;
            if (obj.GetType() != GetType()) return false;

            return Equals(entity.Key, Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public static bool operator==(Entity<K> a, Entity<K> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null) return false;

            return a.Equals(b);
        }

        public static bool operator!=(Entity<K> a, Entity<K> b)
        {
            return !(a == b);
        }

        public void Fill(DbDataReader reader)
        {
            InternalFill(reader);
            Filled = true;
        }

        /// <param name="reader">DbDataReader with which you can get the columns of the current row</param>
        protected abstract void InternalFill(DbDataReader reader);
    }
}
