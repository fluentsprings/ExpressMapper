using System.Collections.Generic;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests
{
    public class SizeListMapper : ICustomTypeMapper<List<Size>, List<SizeViewModel>>
    {
        public List<SizeViewModel> Map(IMappingContext<List<Size>, List<SizeViewModel>> context)
        {
            var dest = context.Destination ?? new List<SizeViewModel>(context.Source.Count);
            var destExists = dest.Count > 0;

            for (var i = 0; i < context.Source.Count; i++)
            {
                var size = context.Source[i];

                var sizeVm = !destExists ? new SizeViewModel() : dest[i];

                sizeVm.Id = size.Id;
                sizeVm.Alias = size.Alias;
                sizeVm.Name = size.Name;
                sizeVm.SortOrder = size.SortOrder;
                if (destExists)
                {
                    dest[i] = sizeVm;
                }
                else
                {
                    dest.Add(sizeVm);
                }
            }
            return dest;
        }
    }
}
