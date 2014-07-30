namespace Database
{
    interface ISafeDataRecord
    {
        T Parse<T>(string column);
    }
}
