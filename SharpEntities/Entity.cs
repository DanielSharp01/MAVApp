using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace SharpEntities
{
    /// <summary>
    /// Represents an abstract entity in the database which has an Key
    /// </summary>
    public abstract class Entity<K>
    {
        /// <summary>
        /// Database Key
        /// </summary>
        public K Key { get; set; }

        /// <summary>
        /// Tells whether this object was filled from the database or not
        /// </summary>
        public bool Filled { set; get; } = false;

        public bool Changed { get; private set; }

        /// <param name="reader">DbDataReader with which you can get the columns of the current row</param>
        public abstract void Fill(DbDataReader reader);

        public abstract void Fill(Entity<K> other);

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
