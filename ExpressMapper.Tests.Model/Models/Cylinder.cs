using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Cylinder
    {
        public Guid Id { get; set; }
        public decimal Capacity { get; set; }
        public Engine Engine { get; set; }
    }
}
