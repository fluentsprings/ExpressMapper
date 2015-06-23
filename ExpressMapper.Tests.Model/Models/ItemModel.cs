using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.Models
{
    public class ItemModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<SubItem> SubItems { get; set; }
    }
}
