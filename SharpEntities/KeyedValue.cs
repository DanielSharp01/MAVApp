namespace SharpEntities
{
    public class KeyedValue<K, V>
    {
        public K Key { get; }
        public V Value { get; set; }

        public KeyedValue(K key)
        {
            Key = key;
        }
    }
}
