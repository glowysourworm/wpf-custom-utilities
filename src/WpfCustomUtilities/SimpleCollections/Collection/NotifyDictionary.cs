using System;
using System.Collections;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Interface;
using WpfCustomUtilities.SimpleCollections.Collection.Interface;

namespace WpfCustomUtilities.SimpleCollections.Collection
{
    /// <summary>
    /// SimpleDictionary implementation that responds to the INotifyPropertyChanged interface
    /// </summary>
    public class NotifyDictionary<K, V> : IDictionary<K, V>, IRecursiveSerializable where K : INotifyDictionaryKey
    {
        SimpleDictionary<int, V> _valueDict;
        SimpleDictionary<int, K> _keyDict;

        public NotifyDictionary()
        {
            _keyDict = new SimpleDictionary<int, K>();
            _valueDict = new SimpleDictionary<int, V>();
        }

        public NotifyDictionary(IPropertyReader reader)
        {
            _keyDict = reader.Read<SimpleDictionary<int, K>>("Keys");
            _valueDict = reader.Read<SimpleDictionary<int, V>>("Values");

            foreach (var key in _keyDict.Keys)
            {
                if (!_valueDict.ContainsKey(key))
                    throw new Exception("Invalid de-serialized key / value pairs:  NotifyDictionary.cs");
            }
        }

        ~NotifyDictionary()
        {
            Clear();
        }

        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("Keys", _keyDict);
            writer.Write("Values", _valueDict);
        }

        public V this[K key]
        {
            get { return _valueDict[key.GetHashCode()]; }
            set { SetImpl(key, value); }
        }

        public ICollection<K> Keys
        {
            get { return _keyDict.Values; }
        }

        public ICollection<V> Values
        {
            get { return _valueDict.Values; }
        }

        public int Count
        {
            get { return _valueDict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(K key, V value)
        {
            AddImpl(key, value);
        }

        public void Add(KeyValuePair<K, V> item)
        {
            AddImpl(item.Key, item.Value);
        }

        public void Clear()
        {
            ClearImpl();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            var hashCode = item.GetHashCode();

            return _keyDict.ContainsKey(hashCode);
        }

        public bool ContainsKey(K key)
        {
            return _keyDict.ContainsKey(key.GetHashCode());
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            throw new Exception("Can't iterate the notify dictionary. Please use the values iterator");
        }

        public bool Remove(K key)
        {
            return RemoveImpl(key);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            return RemoveImpl(item.Key);
        }

        public bool TryGetValue(K key, out V value)
        {
            return _valueDict.TryGetValue(key.GetHashCode(), out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("Please use the Keys / Values enumerators for NotifyDictionary");
        }

        private void AddImpl(K key, V value)
        {
            if (_keyDict.ContainsKey(key.GetHashCode()))
                throw new Exception("Trying to add duplicate key to NotifyDictionary");

            key.HashCodeChangedEvent -= OnHashCodeChanged;
            key.HashCodeChangedEvent += OnHashCodeChanged;

            _keyDict.Add(key.GetHashCode(), key);
            _valueDict.Add(key.GetHashCode(), value);
        }

        private void SetImpl(K key, V value)
        {
            if (!_keyDict.ContainsKey(key.GetHashCode()))
                throw new Exception("NotifyDictionary does not contain specified set key");

            key.HashCodeChangedEvent -= OnHashCodeChanged;
            key.HashCodeChangedEvent += OnHashCodeChanged;

            _valueDict[key.GetHashCode()] = value;
        }

        private bool RemoveImpl(K key)
        {
            if (!_keyDict.ContainsKey(key.GetHashCode()))
                throw new Exception("NotifyDictionary does not contain key for removal");

            key.HashCodeChangedEvent -= OnHashCodeChanged;

            var success = true;

            success &= _keyDict.Remove(key.GetHashCode());
            success &= _valueDict.Remove(key.GetHashCode());

            return success;
        }

        private void ClearImpl()
        {
            foreach (var key in _keyDict.Values)
                key.HashCodeChangedEvent -= OnHashCodeChanged;

            _keyDict.Clear();
            _valueDict.Clear();
        }

        private void OnHashCodeChanged(INotifyDictionaryKey sender, int oldHashCode, int newHashCode)
        {
            if (!_keyDict.ContainsKey(oldHashCode))
                throw new Exception("NotifyDictionary internal collections do not have reference to the INotifyPropertyChanged key");

            if (!(sender is K))
                throw new Exception("Invalid type or usage of INotifyDictionaryKey interface");

            // Update the item's hash code
            var itemValue = _valueDict[oldHashCode];

            _keyDict.Remove(oldHashCode);
            _valueDict.Remove(oldHashCode);

            _keyDict.Add(newHashCode, (K)sender);
            _valueDict.Add(newHashCode, itemValue);
        }
    }
}
