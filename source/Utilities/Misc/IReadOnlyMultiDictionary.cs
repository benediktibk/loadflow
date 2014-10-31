using System.Collections.Generic;

namespace Misc
{
    public interface IReadOnlyMultiDictionary<in TKey, out TValue>
    {
        IReadOnlyList<TValue> Get(TKey key);
        TValue GetOnly(TKey key);
    }
}
