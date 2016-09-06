using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Trip
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CategoryTrip Category { get; set; }
    }
}
