using ExpressMapper.Tests.Models;
using ExpressMapper.Tests.ViewModels;

namespace ExpressMapper.Tests.CustomMappers
{
    public class SizeMapper : ICustomTypeMapper<Size, SizeViewModel>
    {
        public SizeViewModel Map(Size src)
        {
            return new SizeViewModel
            {
                Id = src.Id,
                Alias = src.Alias,
                Name = src.Name,
                SortOrder = src.SortOrder
            };
        }
    }
}
