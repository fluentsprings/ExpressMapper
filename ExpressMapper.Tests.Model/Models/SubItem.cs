using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class SubItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Unit[] Units { get; set; }
    }
}
