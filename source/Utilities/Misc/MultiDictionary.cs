using System;
using System.Collections.Generic;
using System.Linq;

namespace Misc
{
    public class MultiDictionary<TKey, TValue> : IReadOnlyMultiDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> _values;

        public MultiDictionary()
        {
            _values = new Dictionary<TKey, List<TValue>>();
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public IReadOnlyList<TValue> Get(TKey key)
        {
            List<TValue> result;
            var found = _values.TryGetValue(key, out result);
            return found ? _values[key] : new List<TValue>();
        }

        public TValue GetOnly(TKey key)
        {
            var values = _values[key];

            if (values.Count > 1)
                throw new InvalidOperationException("key has more than one value");

            return values.First();
        }

        public void Add(TKey key, TValue value)
        {
            if (!_values.ContainsKey(key))
                _values.Add(key, new List<TValue>());

            _values[key].Add(value);
        }

        public bool Contains(TKey key)
        {
            return _values.ContainsKey(key);
        }
    }
}
