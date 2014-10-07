namespace DatabaseHelper
{
    public interface ISafeDatabaseRecord
    {
        T Parse<T>(string column);
    }
}
