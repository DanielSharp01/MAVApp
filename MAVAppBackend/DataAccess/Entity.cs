using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    /// <summary>
    /// Represents an abstract entity in the database which has an ID
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Database ID
        /// </summary>
        public int ID { protected set; get; }

        /// <summary>
        /// Tells whether this object was filled from the database or not
        /// </summary>
        public bool Filled { protected set; get; } = false;

        /// <param name="id">Database ID</param>
        public Entity(int id)
        {
            ID = id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (!(obj is Entity entity)) return false;
            if (obj.GetType() != GetType()) return false;

            return entity.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public static bool operator==(Entity a, Entity b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(Entity a, Entity b)
        {
            return !a.Equals(b);
        }

        // Fill method should be implemented in children
    }
}
