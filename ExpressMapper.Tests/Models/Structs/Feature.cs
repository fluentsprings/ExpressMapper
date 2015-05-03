using System;

namespace ExpressMapper.Tests.Models.Structs
{
    public struct Feature
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Rank { get; set; }
    }
}
