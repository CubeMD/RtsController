using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class SerializableKeyValuePair<T1, T2>
    {
        public T1 key;
        public T2 value;
    }
    
    [Serializable]
    public class SerializableDictionary<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>, ISerializationCallbackReceiver
    {
        [SerializeField] 
        private List<SerializableKeyValuePair<T1, T2>> data = new List<SerializableKeyValuePair<T1, T2>>();
        
        private Dictionary<T1, T2> internalDictionary = null;
        private Dictionary<T1, T2> InternalDictionary
        {
            get
            {
                if (internalDictionary == null)
                {
                    internalDictionary = new Dictionary<T1, T2>(data.Count);

                    foreach (SerializableKeyValuePair<T1, T2> keyValuePair in data)
                    {
                        if (!internalDictionary.TryAdd(keyValuePair.key, keyValuePair.value))
                        {
                            Debug.LogWarning("Duplicate key detected in serialized dictionary data");
                        }
                    }
                }

                return internalDictionary;
            }
        }

        public IEnumerable<T1> Keys => InternalDictionary.Keys;
        public IEnumerable<T2> Values => InternalDictionary.Values;
        public int Count => InternalDictionary.Count;
        
        public T2 this[T1 key]
        {
            get => InternalDictionary[key];
            set => InternalDictionary[key] = value;
        }

        public bool TryAdd(T1 key, T2 value)
        {
            return InternalDictionary.TryAdd(key, value);
        }

        public void Add(T1 key, T2 value)
        {
            InternalDictionary.Add(key, value);
        }
        
        public bool ContainsKey(T1 key)
        {
            return InternalDictionary.ContainsKey(key);
        }

        public bool ContainsValue(T2 value)
        {
            return InternalDictionary.ContainsValue(value);
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return InternalDictionary.TryGetValue(key, out value);
        }

        public T2 GetValueOrDefault(T1 key, T2 defaultValue)
        {
            return InternalDictionary.GetValueOrDefault(key, defaultValue);
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return InternalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#if UNITY_EDITOR
        public void EDITOR_AddPairToSerializedList(T1 key, T2 value)
        {
            SerializableKeyValuePair<T1, T2> pair = new SerializableKeyValuePair<T1, T2>
            {
                key = key,
                value = value
            };
            
            data.Add(pair);
        }
        
        public void EDITOR_RemovePairFromSerializedList(T1 key)
        {
            SerializableKeyValuePair<T1, T2> pair = data.FirstOrDefault(x => x.key.Equals(key));
            if (pair != null)
            {
                data.Remove(pair);
            }
        }
#endif

        public void OnBeforeSerialize() { }
        
        public void OnAfterDeserialize()
        {
            internalDictionary = null;
        }
    }
}