using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests
{
    public class SizeMapper : ICustomTypeMapper<Size, SizeViewModel>
    {
        public SizeViewModel Map(IMappingContext<Size, SizeViewModel> context)
        {
            var sizeViewModel = context.Destination ?? new SizeViewModel();

            sizeViewModel.Id = context.Source.Id;
            sizeViewModel.Alias = context.Source.Alias;
            sizeViewModel.Name = context.Source.Name;
            sizeViewModel.SortOrder = context.Source.SortOrder;
            return sizeViewModel;
        }
    }
}
