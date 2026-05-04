using System.Collections.Generic;

namespace UHFPS.Runtime
{
    /// <summary>
    /// A buffer that stores data that can be used for serializing data or do other operations.
    /// <br>Implements <see cref="Dictionary{TKey, TValue}"/> where the key is <see cref="string"/> and the value is <see cref="object"/>.</br>
    /// </summary>
    public class StorableCollection : Dictionary<string, object> 
    {
        public T GetT<T>(string key)
        {
            if (TryGetValue(key, out var value))
                if (value is T valueT)
                    return valueT;

            throw new System.NullReferenceException($"Could not find item with key '{key}' or could not convert to type '{typeof(T).Name}'.");
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (TryGetValue(key, out var valueO))
            {
                if (valueO is T valueT)
                {
                    value = valueT;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}