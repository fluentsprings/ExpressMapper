using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class FlattenFatherSonSimpleDto
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public FlattenSimpleClass Son { get; set; }
    }
}