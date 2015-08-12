using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class EngineViewModel : IEquatable<EngineViewModel>
    {
        public Guid Id { get; set; }
        public string Capacity { get; set; }
        public List<CylinderViewModel> Cylinders { get; set; }

        public bool Equals(EngineViewModel other)
        {
            var subItems = true;
            if (Cylinders != null && other.Cylinders != null)
            {
                if (Cylinders.Count == other.Cylinders.Count)
                {
                    if (Cylinders.Where((t, i) => !t.Equals(other.Cylinders[i])).Any())
                    {
                        subItems = false;
                    }
                }
            }
            return Id == other.Id && Capacity == other.Capacity && subItems;
        }
    }
}
