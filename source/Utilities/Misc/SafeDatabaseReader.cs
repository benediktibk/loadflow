using System;
using System.Data.Common;

namespace Misc
{
    public class SafeDatabaseReader : IDisposable, ISafeDatabaseRecord
    {
        private readonly DbDataReader _internalReader;

        public SafeDatabaseReader(DbDataReader dataReader)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            _internalReader = dataReader;
        }

        public void Dispose()
        {
            if (_internalReader == null)
                return;

            _internalReader.Close();
            _internalReader.Dispose();
        }

        public T Parse<T>(string column)
        {
            var columnIndex = _internalReader.GetOrdinal(column);

            if (_internalReader.IsDBNull(columnIndex))
                return default(T);

            var value = _internalReader.GetValue(columnIndex).ToString();
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public bool Next()
        {
            return _internalReader.Read();
        }
    }
}
