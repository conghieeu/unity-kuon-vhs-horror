using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UHFPS.Tools
{
    public class MultiKeyDictionary<K, L, V>
    {
        internal readonly Dictionary<K, V> baseDictionary = new Dictionary<K, V>();
        internal readonly Dictionary<L, K> subDictionary = new Dictionary<L, K>();
        internal readonly Dictionary<K, L> primaryToSubkeyMapping = new Dictionary<K, L>();
        internal readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        public V this[L subKey]
        {
            get
            {
                if (TryGetValue(subKey, out V item))
                    return item;

                throw new KeyNotFoundException("sub key not found: " + subKey.ToString());
            }
        }

        public V this[K primaryKey]
        {
            get
            {
                if (TryGetValue(primaryKey, out V item))
                    return item;

                throw new KeyNotFoundException("primary key not found: " + primaryKey.ToString());
            }
        }

        public void Associate(L subKey, K primaryKey)
        {
            readerWriterLock.EnterUpgradeableReadLock();

            try
            {
                if (!baseDictionary.ContainsKey(primaryKey))
                    throw new KeyNotFoundException(string.Format("The base dictionary does not contain the key '{0}'", primaryKey));

                if (primaryToSubkeyMapping.ContainsKey(primaryKey))
                {
                    readerWriterLock.EnterWriteLock();

                    try
                    {
                        if (subDictionary.ContainsKey(primaryToSubkeyMapping[primaryKey]))
                        {
                            subDictionary.Remove(primaryToSubkeyMapping[primaryKey]);
                        }

                        primaryToSubkeyMapping.Remove(primaryKey);
                    }
                    finally
                    {
                        readerWriterLock.ExitWriteLock();
                    }
                }

                subDictionary[subKey] = primaryKey;
                primaryToSubkeyMapping[primaryKey] = subKey;
            }
            finally
            {
                readerWriterLock.ExitUpgradeableReadLock();
            }
        }

        public bool TryGetValue(L subKey, out V val)
        {
            val = default;
            readerWriterLock.EnterReadLock();

            try
            {
                if (subDictionary.TryGetValue(subKey, out K primaryKey))
                {
                    return baseDictionary.TryGetValue(primaryKey, out val);
                }
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }

            return false;
        }

        public bool TryGetValue(K primaryKey, out V val)
        {
            readerWriterLock.EnterReadLock();

            try
            {
                return baseDictionary.TryGetValue(primaryKey, out val);
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public bool ContainsKey(L subKey)
        {
            return TryGetValue(subKey, out _);
        }

        public bool ContainsKey(K primaryKey)
        {
            return TryGetValue(primaryKey, out _);
        }

        public void Remove(K primaryKey)
        {
            readerWriterLock.EnterWriteLock();

            try
            {
                if (primaryToSubkeyMapping.ContainsKey(primaryKey))
                {
                    if (subDictionary.ContainsKey(primaryToSubkeyMapping[primaryKey]))
                    {
                        subDictionary.Remove(primaryToSubkeyMapping[primaryKey]);
                    }

                    primaryToSubkeyMapping.Remove(primaryKey);
                }

                baseDictionary.Remove(primaryKey);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public void Remove(L subKey)
        {
            readerWriterLock.EnterWriteLock();

            try
            {
                baseDictionary.Remove(subDictionary[subKey]);

                primaryToSubkeyMapping.Remove(subDictionary[subKey]);

                subDictionary.Remove(subKey);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public void Add(K primaryKey, V val)
        {
            readerWriterLock.EnterWriteLock();

            try
            {
                baseDictionary.Add(primaryKey, val);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public void Add(K primaryKey, L subKey, V val)
        {
            Add(primaryKey, val);
            Associate(subKey, primaryKey);
        }

        public V[] CloneValues()
        {
            readerWriterLock.EnterReadLock();

            try
            {
                V[] values = new V[baseDictionary.Values.Count];
                baseDictionary.Values.CopyTo(values, 0);
                return values;
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public List<V> Values
        {
            get
            {
                readerWriterLock.EnterReadLock();

                try
                {
                    return baseDictionary.Values.ToList();
                }
                finally
                {
                    readerWriterLock.ExitReadLock();
                }
            }
        }

        public K[] ClonePrimaryKeys()
        {
            readerWriterLock.EnterReadLock();

            try
            {
                K[] values = new K[baseDictionary.Keys.Count];
                baseDictionary.Keys.CopyTo(values, 0);
                return values;
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public L[] CloneSubKeys()
        {
            readerWriterLock.EnterReadLock();

            try
            {
                L[] values = new L[subDictionary.Keys.Count];
                subDictionary.Keys.CopyTo(values, 0);
                return values;
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        public void Clear()
        {
            readerWriterLock.EnterWriteLock();

            try
            {
                baseDictionary.Clear();
                subDictionary.Clear();
                primaryToSubkeyMapping.Clear();
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                readerWriterLock.EnterReadLock();

                try
                {
                    return baseDictionary.Count;
                }
                finally
                {
                    readerWriterLock.ExitReadLock();
                }
            }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            readerWriterLock.EnterReadLock();

            try
            {
                return baseDictionary.GetEnumerator();
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }
    }
}