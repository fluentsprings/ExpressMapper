using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ExpressMapper.Tests.Models
{
    public class Supplier
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime AgreementDate { get; set; }
        public int Rank { get; set; }

        public List<Size> Sizes { get; set; }
    }
}
