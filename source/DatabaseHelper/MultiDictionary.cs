using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseHelper
{
    public class MultiDictionary<TKey, TValue> : IReadOnlyMultiDictionary<TKey, TValue>
    {
        #region variables

        private Dictionary<TKey, List<TValue>> _values; 

        #endregion

        #region constructor

        public MultiDictionary()
        {
            _values = new Dictionary<TKey, List<TValue>>();
        }

        #endregion

        #region public functions

        public IReadOnlyList<TValue> Get(TKey key)
        {
            return _values[key];
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

        #endregion
    }
}
