using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.Models
{
    public class SpecialGift : Gift
    {
        public new SpecialPerson Recipient { get; set; }
    }
}
