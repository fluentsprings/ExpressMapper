using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class FlattenFatherSonGrandsonLowerCaseDto
    {
        public int myInt { get; set; }
        public string mystring { get; set; }

        public int Sonmyint { get; set; }
        public string sonMyString { get; set; }

        public int? sonGrandsonMyInt { get; set; }
        public string SongrandsonmYstring { get; set; }
    }
}