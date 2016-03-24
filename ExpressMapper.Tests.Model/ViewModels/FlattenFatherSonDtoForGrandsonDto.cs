using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class FlattenFatherSonDtoForGrandsonDto
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public int SonMyInt { get; set; }
        public string SonMyString { get; set; }

        public FlattenSimpleClass SonGrandson { get; set; }
    }
}