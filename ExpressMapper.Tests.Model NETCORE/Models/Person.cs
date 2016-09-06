using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Person : Contact
    {
        public Person()
        {
            IsOrganization = false;
            IsPerson = true;
        }

        public string Name { get; set; }
        public Person Relative { get; set; }
    }
}
