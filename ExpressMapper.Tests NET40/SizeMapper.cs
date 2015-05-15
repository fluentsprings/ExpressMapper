using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests
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
