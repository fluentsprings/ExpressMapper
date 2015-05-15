using System;
using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Tests.Model.Enums;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class ProductViewModel : IEquatable<ProductViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ProductOptionViewModel> Options { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? WarehouseOn { get; set; }
        public string Ean { get; set; }
        public GenderTypes? OptionalGender { get; set; }
        public BrandViewModel Brand { get; set; }
        public SupplierViewModel Supplier { get; set; }

        public bool Equals(ProductViewModel other)
        {
            var optionsEquals = ((Options != null && other.Options != null) && Options.Count == other.Options.Count) || ((other.Options == null && Options == null));
            if (optionsEquals && Options != null && other.Options != null && Options.Where((t, i) => !t.Equals(other.Options[i])).Any())
            {
                optionsEquals = false;
            }

            return Id == other.Id && Name == other.Name && Description == other.Description && optionsEquals &&
                   CreatedOn == other.CreatedOn && WarehouseOn == other.WarehouseOn && Ean == Ean &&
                   OptionalGender == other.OptionalGender &&
                   ((Brand == null && other.Brand == null) || Brand.Equals(other.Brand)) &&
                   ((Supplier == null && other.Supplier == null) || Supplier.Equals(other.Supplier));
        }
    }
}
