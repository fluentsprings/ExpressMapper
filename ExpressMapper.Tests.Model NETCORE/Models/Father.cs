using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class Father
    {

        public int MyInt { get; set; }
        public string MyString { get; set; }

        public Son Son { get; set; }


        public static Father CreateOne()
        {
            return new Father
            {
                MyInt = 1,
                MyString = "Father",
                Son = Son.CreateOne()
            };
        }

        public static IEnumerable<Father> CreateMany(int num = 5)
        {
            for (int i = 0; i < num; i++)
            {
                yield return CreateOne();
            }
        }
    }
}