using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TableViewModel : IEquatable<TableViewModel>
    {
        public Guid Id;
        public string Name;
        public List<BrandViewModel> Brands;
        public CountryViewModel Manufacturer;
        public List<SizeViewModel> Sizes;
        public bool Equals(TableViewModel other)
        {
            var equals = ((Sizes != null && other.Sizes != null) && Sizes.Count == other.Sizes.Count) || ((other.Sizes == null && Sizes == null));
            if (equals && Sizes != null && other.Sizes != null && Sizes.Where((t, i) => !t.Equals(other.Sizes[i])).Any())
            {
                equals = false;
            }

            var equalsBrands = ((Brands != null && other.Brands != null) && Brands.Count == other.Brands.Count) || ((other.Brands == null && Brands == null));
            if (equalsBrands && Brands != null && other.Brands != null && Brands.Where((t, i) => !t.Equals(other.Brands[i])).Any())
            {
                equalsBrands = false;
            }

            return Id == other.Id && Name == other.Name && equals && equalsBrands && Manufacturer.Equals(other.Manufacturer);
        }
    }
}
