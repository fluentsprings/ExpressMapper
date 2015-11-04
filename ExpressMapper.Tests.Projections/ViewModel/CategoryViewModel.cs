using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<ProductViewModel> Products { get; set; }
    }
}
