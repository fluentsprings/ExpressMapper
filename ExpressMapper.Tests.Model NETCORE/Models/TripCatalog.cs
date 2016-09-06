using System;
using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests.Model.Models
{
    public class TripCatalog
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TripType TripType { get; set; }
    }
}
