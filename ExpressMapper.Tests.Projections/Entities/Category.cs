using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<Product> Products { get; set; }
    }
}
