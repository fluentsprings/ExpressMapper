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


        public static FlattenFatherSonGrandsonDto CreateOne(Random rand = null)
        {
            rand = rand ?? new Random();
            return new FlattenFatherSonGrandsonDto
            {
                MyInt = rand.Next(),
                MyString = "Father",
                SonMyInt = 2,
                SonMyString = "Son",
                SonGrandsonMyInt = 3,
                SonGrandsonMyString = "Grandson"
            };
        }
    }
}