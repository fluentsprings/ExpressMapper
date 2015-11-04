using System;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Dimensions { get; set; }
        public ProductVariantViewModel Variant { get; set; }
    }
}
