using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class CategoryTripViewModel : IEquatable<CategoryTripViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TripCatalogViewModel Catalog { get; set; }
        public bool Equals(CategoryTripViewModel other)
        {
            return Id == other.Id && Name == other.Name && Catalog.Equals(other.Catalog);
        }
    }
}
