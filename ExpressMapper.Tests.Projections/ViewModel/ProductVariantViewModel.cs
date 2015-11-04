using System;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class ProductVariantViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public SizeViewModel Size { get; set; }
    }
}
