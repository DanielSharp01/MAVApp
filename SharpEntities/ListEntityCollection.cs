using System.Collections.Generic;

namespace SharpEntities
{
    public class ListEntityCollection<B, K, E> : EntityCollection<B, K, E> where E : Entity<K>
    {
        private List<E> entities = new List<E>();

        public override IEnumerator<E> GetEnumerator()
        {
            return entities.GetEnumerator();
        }

        public override void Add(E item)
        {
            entities.Add(item);
        }

        public override void Clear()
        {
            entities.Clear();
        }

        public E this[int i]
        {
            get => entities[i];
            set { entities[i] = value; }
        }

        public override bool Contains(E item)
        {
            return entities.Contains(item);
        }

        public override bool Remove(E item)
        {
            return entities.Remove(item);
        }

        public override int Count => entities.Count;
    }
}
