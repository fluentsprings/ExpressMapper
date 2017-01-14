namespace ExpressMapper
{
    public interface IMappingContext<T, TN>
    {
        T Source { get; set; }
        TN Destination { get; set; }
    }
}
