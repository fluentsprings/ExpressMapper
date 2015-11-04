using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class ProductVariant
    {
        public ProductVariant()
        {
            Products = new List<Product>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }

        public Guid? SizeId { get; set; }

        public virtual Size Size { get; set; }
        public virtual List<Product> Products { get; set; }
    }
}
