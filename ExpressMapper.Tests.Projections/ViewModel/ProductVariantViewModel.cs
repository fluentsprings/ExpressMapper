using System;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class ProductVariantViewModel : IEquatable<ProductVariantViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public SizeViewModel Size { get; set; }

        public bool Equals(ProductVariantViewModel other)
        {
            return Id == other.Id && Name == other.Name && Color == other.Color && ((Size == null && other.Size == null) || Size.Equals(other.Size));
        }
    }
}
