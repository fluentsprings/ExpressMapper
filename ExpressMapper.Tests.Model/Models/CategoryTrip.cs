using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class CategoryTrip
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TripCatalog Catalog { get; set; }
    }
}
