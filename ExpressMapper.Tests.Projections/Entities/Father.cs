using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class Father
    {

        public int Id { get; set; }

        public int MyInt { get; set; }
        public string MyString { get; set; }

        public Son Son { get; set; }


        public static Father CreateOne(int count)
        {
            return new Father
            {
                MyInt = count,
                MyString = "Father",
                Son = Son.CreateOne(count)
            };
        }
    }
}