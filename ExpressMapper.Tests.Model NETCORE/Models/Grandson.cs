using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Grandson
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public static Grandson CreateOne()
        {
            return new Grandson
            {
                MyInt = 3,
                MyString = "Grandson"
            };
        }
    }
}