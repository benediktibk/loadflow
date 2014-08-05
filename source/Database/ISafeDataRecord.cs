namespace Database
{
    public interface ISafeDataRecord
    {
        T Parse<T>(string column);
    }
}
