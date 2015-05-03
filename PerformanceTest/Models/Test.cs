using System;
using System.Collections.Generic;

namespace PerformanceTest.Models
{
    public class Test
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public decimal Weight { get; set; }
        public DateTime Created { get; set; }
        public Guid ListId { get; set; }
        public int Type { get; set; }
        public Product Product { get; set; }
        public Product SpareProduct { get; set; }
        public List<Product> Products { get; set; }
    }
}
