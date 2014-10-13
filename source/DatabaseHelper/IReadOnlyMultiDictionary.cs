using System.Collections.Generic;

namespace DatabaseHelper
{
    public interface IReadOnlyMultiDictionary<in TKey, out TValue>
    {
        IReadOnlyList<TValue> Get(TKey key);
        TValue GetOnly(TKey key);
    }
}
