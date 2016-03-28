using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class Son
    {

        public int MyInt { get; set; }
        public string MyString { get; set; }

        public Grandson Grandson { get; set; }

        public static Son CreateOne()
        {
            return new Son
            {
                MyInt = 2,
                MyString = "Son",
                Grandson = Grandson.CreateOne()
            };
        }

        public static IEnumerable<Son> CreateMany(int num = 5)
        {
            for (int i = 0; i < num; i++)
            {
                yield return CreateOne();
            }
        }
    }
}