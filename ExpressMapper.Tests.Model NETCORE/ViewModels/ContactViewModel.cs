using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class ContactViewModel
    {
        public Guid Id { get; set; }

        public bool IsPerson { get; set; }

        public bool IsOrganization { get; set; }

        protected bool ContactEquals(ContactViewModel other)
        {
            return Id == other.Id && IsPerson == other.IsPerson && IsOrganization == other.IsOrganization;
        }
    }
}
