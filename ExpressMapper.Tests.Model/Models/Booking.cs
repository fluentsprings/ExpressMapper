using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Composition Composition { get; set; }
    }
}
