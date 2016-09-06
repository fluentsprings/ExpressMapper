using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class ProductOption
    {
        public Guid Id { get; set; }
        public string Color { get; set; }
        public Size Size { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal? Weight { get; set; }
        public int Stock { get; set; }
        public decimal DiscountedPrice { get { return Math.Floor(Price*Discount/100); } }
        public List<City> Cities { get; set; }
    }
}
