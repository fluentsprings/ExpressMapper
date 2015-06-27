using System;
using System.Collections.Generic;

namespace Benchmarks.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public decimal Weight { get; set; }
        public string Description { get; set; }
        public List<ProductVariant> Options { get; set; }
        public ProductVariant DefaultOption { get; set; }
    }
}
