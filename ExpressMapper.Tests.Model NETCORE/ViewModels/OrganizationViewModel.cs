using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class OrganizationViewModel : ContactViewModel, IEquatable<PersonViewModel>
    {
        public OrganizationViewModel()
        {
            IsOrganization = true;
            IsPerson = false;
        }

        public string Name { get; set; }
        public OrganizationViewModel Relative { get; set; }

        public bool Equals(PersonViewModel other)
        {
            return ContactEquals(other) && Name == other.Name && (Relative == null || Relative.Equals(other.Relative));
        }
    }
}
