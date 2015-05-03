using System;
using System.Collections.Generic;

namespace PerformanceTest.ViewModels
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public decimal Weight { get; set; }
        public string Description { get; set; }
        public List<ProductVariantViewModel> Options { get; set; }
        public ProductVariantViewModel DefaultSharedOption { get; set; }
    }
}
