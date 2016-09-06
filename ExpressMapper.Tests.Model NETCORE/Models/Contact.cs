using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.Models
{
    public class Contact
    {
        public Guid Id { get; set; }

        public bool IsPerson { get; set; }

        public bool IsOrganization { get; set; }
    }
}
