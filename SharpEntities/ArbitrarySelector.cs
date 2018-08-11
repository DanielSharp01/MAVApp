using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SharpEntities
{
    public abstract class ArbitrarySelector<K, V> 
    {
        protected Dictionary<K, V> selectBatch;
        protected BatchSelectStrategy batchStrategy;

        public virtual void BeginSelect(BatchSelectStrategy batchSelectStrategy = BatchSelectStrategy.MultiKey)
        {
            if (selectBatch != null) return;
            batchStrategy = batchSelectStrategy;
            selectBatch = new Dictionary<K, V>();
        }

        public virtual void EndSelect()
        {
            if (selectBatch == null) return;

            if (selectBatch.Count == 0)
            {
                selectBatch = null;
                return;
            }

            DbDataReader reader = (batchStrategy == BatchSelectStrategy.MultiKey) ? SelectByKeys(selectBatch.Keys.ToList()) : SelectAll();

            while (reader.Read())
            {
                K key = GetKey(reader);
                if (!selectBatch.ContainsKey(key)) continue;

                var value = selectBatch[key];
                Fill(value, reader);
            }

            reader.Close();
            selectBatch = null;
        }

        public virtual V GetByKey(K key)
        {
            if (selectBatch != null && selectBatch.ContainsKey(key))
            {
                return selectBatch[key];
            }

            V value = CreateValue(key);
            FillByKey(value);
            return value;
        }

        public virtual void FillByKey(V value)
        {
            if (selectBatch == null)
            {
                FillByKeySingle(value);
            }
            else
            {
                K key = GetKey(value);
                if (!selectBatch.ContainsKey(key))
                    selectBatch[key] = value;
            }
        }

        protected virtual void FillByKeySingle(V value)
        {
            DbDataReader reader = SelectByKey(GetKey(value));
            if (reader.Read())
            {
                Fill(value, reader);
            }
            reader.Close();
        }

        protected abstract DbDataReader SelectByKey(K key);

        protected abstract DbDataReader SelectByKeys(IList<K> keys);

        protected abstract DbDataReader SelectAll();

        protected abstract K GetKey(V value);

        protected abstract K GetKey(DbDataReader reader);

        protected abstract V CreateValue(K key);

        protected abstract void Fill(V value, DbDataReader reader);
    }
}
