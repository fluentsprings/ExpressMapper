using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class SubItemViewModel : IEquatable<SubItemViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Collection<UnitViewModel> Units { get; set; }
        public bool Equals(SubItemViewModel other)
        {
            var units = true;
            if (Units != null && other.Units != null)
            {
                if (Units.Count == other.Units.Count)
                {
                    if (Units.Where((t, i) => !t.Equals(other.Units[i])).Any())
                    {
                        units = false;
                    }
                }
            }
            return Id == other.Id && Name == other.Name && units;
        }
    }
}
