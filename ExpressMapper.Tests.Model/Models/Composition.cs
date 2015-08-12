using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Composition
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Booking Booking { get; set; }
    }
}
