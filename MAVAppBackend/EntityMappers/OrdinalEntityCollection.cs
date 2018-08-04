using System;
using System.Collections.Generic;
using MAVAppBackend.Entities;
using SharpEntities;

namespace MAVAppBackend.EntityMappers
{
    public class OrdinalEntityCollection<B, K, E> : EntityCollection<B, K, E> where E : OrdinalEntity<K>
    {
        private SortedList<int, E> orderedEntities = new SortedList<int, E>();
        public override IEnumerator<E> GetEnumerator()
        {
            return orderedEntities.Values.GetEnumerator();
        }

        public override void Add(E item)
        {
            orderedEntities.Add(item.Ordinal, item);
        }

        public override void Clear()
        {
            orderedEntities.Clear();
        }

        public E this[int i]
        {
            get { return orderedEntities[i]; }
            set { orderedEntities[i] = value; }
        }

        public override bool Contains(E item)
        {
            return orderedEntities.ContainsValue(item);
        }

        public override bool Remove(E item)
        {
            return orderedEntities.Remove(item.Ordinal);
        }

        public override int Count => orderedEntities.Count;
    }
}
