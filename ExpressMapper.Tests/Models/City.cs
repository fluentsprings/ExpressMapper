using System;
using ExpressMapper.Tests.Models.Structs;

namespace ExpressMapper.Tests.Models
{
    public class City
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Feature[] Features { get; set; }
    }
}
