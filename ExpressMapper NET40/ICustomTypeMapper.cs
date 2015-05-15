namespace ExpressMapper
{
    public interface ICustomTypeMapper
    {
    }
    public interface ICustomTypeMapper<in T, out TN> : ICustomTypeMapper
    {
        TN Map(T src);
    }
}
