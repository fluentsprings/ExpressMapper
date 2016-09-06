using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.Models
{
    public class Gift
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Person Recipient { get; set; }
    }
}
