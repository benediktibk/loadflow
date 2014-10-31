namespace Misc
{
    public interface ISafeDatabaseRecord
    {
        T Parse<T>(string column);
    }
}
