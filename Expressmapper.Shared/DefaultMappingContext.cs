namespace ExpressMapper
{
    public class DefaultMappingContext<T, TN> : IMappingContext<T,TN>
    {
        public T Source { get; set; }
        public TN Destination { get; set; }
    }
}
