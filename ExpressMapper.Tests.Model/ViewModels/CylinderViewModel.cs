using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class CylinderViewModel : IEquatable<CylinderViewModel>
    {
        public Guid Id { get; set; }
        public decimal Capacity { get; set; }
        public EngineViewModel Engine { get; set; }

        public bool Equals(CylinderViewModel other)
        {
            return Id == other.Id && Capacity == other.Capacity && ((Engine == null && other.Engine == null) || Engine.Equals(other.Engine));
        }
    }
}
