using System.Collections;

namespace Database
{
    public interface IConnectionNetElements
    {
        void Add(INetElement element, int powerNetId);

        void AddList(IList elements, int powerNetId);

        void Update(INetElement element);

        void Remove(INetElement element);

        void RemoveList(IList elements);
    }
}
