using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class SubUnitViewModel : IEquatable<SubUnitViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Equals(SubUnitViewModel other)
        {
            return Id == other.Id && Name == other.Name;
        }
    }
}
