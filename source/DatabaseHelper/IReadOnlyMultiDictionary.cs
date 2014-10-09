using System.Collections.Generic;

namespace DatabaseHelper
{
    interface IReadOnlyMultiDictionary<Key, Value>
    {
        IReadOnlyList<Value> Get(Key key); 
    }
}
