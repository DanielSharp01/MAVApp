using System.Collections;
using System.Collections.Generic;

namespace SharpEntities
{
    public abstract class EntityCollection<B, K, E> : ICollection<E> where E : Entity<K>
    {
        public B Key { get; set; }
        public bool Filled { get; set; }

        public abstract IEnumerator<E> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Add(E item);

        public abstract void Clear();

        public abstract bool Contains(E item);

        public void CopyTo(E[] array, int arrayIndex)
        {
            foreach (var entity in this)
            {
                array[arrayIndex++] = entity;
            }
        }

        public abstract bool Remove(E item);

        public abstract int Count { get; }

        public bool IsReadOnly => false;
    }
}
