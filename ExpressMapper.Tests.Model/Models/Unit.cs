using System;
using System.Collections.ObjectModel;

namespace ExpressMapper.Tests.Model.Models
{
    public class Unit
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Collection<SubUnit> SubUnits { get; set; }
    }
}
