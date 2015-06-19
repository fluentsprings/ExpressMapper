using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class TestModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public decimal? Weight { get; set; }
        public long Height { get; set; }
        public Country Country { get; set; }
        public List<Size> Sizes { get; set; }
        public DateTime Created { get; set; }
    }
}
