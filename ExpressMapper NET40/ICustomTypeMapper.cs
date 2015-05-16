namespace ExpressMapper
{
    public interface ICustomTypeMapper
    {
    }

    /// <summary>
    /// Interface to implement custom mapper
    /// </summary>
    public interface ICustomTypeMapper<in T, out TN> : ICustomTypeMapper
    {
        TN Map(T src);
    }
}
