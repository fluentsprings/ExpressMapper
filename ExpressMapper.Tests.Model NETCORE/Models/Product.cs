using System;
using System.Collections.Generic;
using ExpressMapper.Tests.Model.Enums;

namespace ExpressMapper.Tests.Model.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ProductOption> Options { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? WarehouseOn { get; set; }
        public string Ean { get; set; }
        public GenderTypes Gender { get; set; }
        public Brand Brand { get; set; }
        public Supplier Supplier { get; set; }
    }
}
