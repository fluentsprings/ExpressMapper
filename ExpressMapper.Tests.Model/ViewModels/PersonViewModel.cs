using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class PersonViewModel : IEquatable<PersonViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PersonViewModel Relative { get; set; }

        public bool Equals(PersonViewModel other)
        {
            return Id == other.Id && Name == other.Name && (Relative == null || Relative.Equals(other.Relative));
        }
    }
}
