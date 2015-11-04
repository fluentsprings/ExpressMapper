using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class Size
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public List<ProductVariant> ProductVariants { get; set; }
    }
}
