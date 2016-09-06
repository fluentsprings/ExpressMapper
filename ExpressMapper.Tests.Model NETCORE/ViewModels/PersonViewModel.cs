using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class PersonViewModel : ContactViewModel, IEquatable<PersonViewModel>
    {
        public PersonViewModel()
        {
            IsOrganization = false;
            IsPerson = true;
        }

        public string Name { get; set; }
        public PersonViewModel Relative { get; set; }

        public bool Equals(PersonViewModel other)
        {
            return ContactEquals(other) && Name == other.Name && (Relative == null || Relative.Equals(other.Relative));
        }
    }
}
