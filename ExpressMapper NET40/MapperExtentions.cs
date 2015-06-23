namespace ExpressMapper
{
    public static class MapperExtentions
    {
        public static TN MapTo<T, TN>(this T src)
        {
            return Mapper.Map<T, TN>(src);
        }

        public static TN MapTo<T, TN>(this T src, TN dest)
        {
            return Mapper.Map(src, dest);
        }
    }
}
