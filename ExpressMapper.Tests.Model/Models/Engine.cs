using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class Engine
    {
        public Guid Id { get; set; }
        public string Capacity { get; set; }
        public List<Cylinder> Cylinders { get; set; }
    }
}
