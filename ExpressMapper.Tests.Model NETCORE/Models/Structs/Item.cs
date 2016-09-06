using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models.Structs
{
    public struct Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public List<Feature> Features { get; set; }
    }
}
