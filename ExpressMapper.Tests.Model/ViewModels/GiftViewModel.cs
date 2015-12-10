using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class GiftViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public PersonViewModel Recipient { get; set; }

        public bool Equals(GiftViewModel other)
        {
            return Id == other.Id && Name == other.Name && (Recipient == null || Recipient.Equals(other.Recipient));
        }
    }
}
