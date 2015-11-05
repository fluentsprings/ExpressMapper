using System;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class ProductViewModel : IEquatable<ProductViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Dimensions { get; set; }
        public ProductVariantViewModel Variant { get; set; }

        public bool Equals(ProductViewModel other)
        {
            return Id == other.Id && Name == other.Name && Dimensions == other.Dimensions && ((Variant == null && other.Variant == null) || Variant.Equals(other.Variant));
        }
    }
}
