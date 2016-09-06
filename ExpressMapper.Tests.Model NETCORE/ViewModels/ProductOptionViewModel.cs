using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class ProductOptionViewModel : IEquatable<ProductOptionViewModel>
    {
        public Guid Id { get; set; }
        public string Color { get; set; }
        public SizeViewModel Size { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal? Weight { get; set; }
        public int Stock { get; set; }
        public decimal DiscountedPrice { get; set; }
        public List<CityViewModel> Cities { get; set; }
        public bool Equals(ProductOptionViewModel other)
        {
            var citiesEqual = ((Cities != null && other.Cities != null) && Cities.Count == other.Cities.Count) || ((other.Cities == null && Cities == null));
            if (citiesEqual && Cities != null && other.Cities != null && Cities.Where((t, i) => !t.Equals(other.Cities[i])).Any())
            {
                citiesEqual = false;
            }

            return Id == other.Id && Color == other.Color &&
                   (Size == null && other.Size == null || Size.Equals(other.Size)) && Price == other.Price &&
                   Discount == other.Discount && Weight == other.Weight && Stock == other.Stock && DiscountedPrice == other.DiscountedPrice && citiesEqual;
        }
    }
}
