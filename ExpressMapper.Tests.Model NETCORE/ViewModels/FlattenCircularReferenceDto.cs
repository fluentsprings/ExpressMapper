using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class FlattenCircularReferenceDto
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public int SonMyInt { get; set; }
        public string SonMyString { get; set; }

        public int CircularRefMyInt { get; set; }
        public string CircularRefMyString { get; set; }

    }
}