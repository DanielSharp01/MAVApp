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
