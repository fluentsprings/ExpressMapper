namespace ExpressMapper
{
    public interface ICustomTypeMapper
    {
    }

    /// <summary>
    /// Interface to implement custom mapperd
    /// </summary>
    public interface ICustomTypeMapper<in T, out TN> : ICustomTypeMapper
    {
        TN Map(T src);
    }
}
