using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpEntities;

namespace MAVAppBackend.Entities
{
    public abstract class OrdinalEntity<K> : Entity<K>
    {
        protected int ordinal;
        public int Ordinal
        {
            get => ordinal;
            set
            {
                ordinal = value;
                OnChange();
            }
        }
    }
}
