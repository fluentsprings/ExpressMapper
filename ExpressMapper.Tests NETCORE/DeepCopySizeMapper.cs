using ExpressMapper.Tests.Model.Models;

namespace ExpressMapper.Tests
{
    public class DeepCopySizeMapper : ICustomTypeMapper<Size, Size>
    {
        public Size Map(IMappingContext<Size, Size> context)
        {
            var deepCopy = new Size();
            deepCopy.Id = context.Source.Id;
            deepCopy.Alias = context.Source.Alias;
            deepCopy.Name = context.Source.Name;
            deepCopy.SortOrder = context.Source.SortOrder;
            return deepCopy;
        }
    }
}
