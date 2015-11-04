using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class Product
    {
        public Product()
        {
            Categories = new List<Category>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Dimensions { get; set; }

        public Guid? VariantId { get; set; }

        public virtual ProductVariant Variant { get; set; }
        public virtual List<Category> Categories { get; set; }
    }
}
