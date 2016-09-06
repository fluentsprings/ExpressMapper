using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class FlattenFatherSonGrandsonDto
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public int SonMyInt { get; set; }
        public string SonMyString { get; set; }

        public int? SonGrandsonMyInt { get; set; }
        public string SonGrandsonMyString { get; set; }
    }
}