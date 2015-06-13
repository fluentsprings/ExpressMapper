using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class UnitViewModel : IEquatable<UnitViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<SubUnitViewModel> SubUnits { get; set; }
        public bool Equals(UnitViewModel other)
        {
            var subUnits = true;
            if (SubUnits != null && other.SubUnits != null)
            {
                if (SubUnits.Count == other.SubUnits.Count)
                {
                    if (SubUnits.Where((t, i) => !t.Equals(other.SubUnits[i])).Any())
                    {
                        subUnits = false;
                    }
                }
            }
            return Id == other.Id && Name == other.Name && subUnits;
        }
    }
}
