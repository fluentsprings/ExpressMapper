using System;
using System.Collections.Generic;
using PerformanceTest.Enums;

namespace PerformanceTest.ViewModels
{
    public class TestViewModel
    {
        public TestViewModel()
        {
            
        }
        public TestViewModel(string descr)
        {
            Description = descr;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Age { get; set; }
        public decimal Weight { get; set; }
        public DateTime Created { get; set; }
        public Types Type { get; set; }
        public ProductViewModel Product { get; set; }
        public ProductViewModel SpareTheProduct { get; set; }
        public List<ProductViewModel> Products { get; set; }
    }
}
