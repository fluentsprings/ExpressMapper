using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.Models
{
    public class Organization : Contact
    {
        public Organization()
        {
            IsOrganization = true;
            IsPerson = false;
        }

        public string Name { get; set; }
        public Person Relative { get; set; }
    }
}
