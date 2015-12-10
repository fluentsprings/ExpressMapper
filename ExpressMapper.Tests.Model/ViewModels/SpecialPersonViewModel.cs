using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class SpecialPersonViewModel : PersonViewModel
    {
        public int AffectionLevel { get; set; }

        public bool Equals(SpecialPersonViewModel other)
        {
            return base.Equals(other) && AffectionLevel == other.AffectionLevel;
        }
    }
}
