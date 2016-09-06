using System;
using ExpressMapper.Tests.Model.Models.Structs;

namespace ExpressMapper.Tests.Model.Models
{
    public class City
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Feature[] Features { get; set; }
    }
}
