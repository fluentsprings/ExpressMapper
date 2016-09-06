using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class Table
    {
        public Guid Id;
        public string Name;
        public List<Brand> Brands;
        public Country Manufacturer;
        public List<Size> Sizes;
    }
}
