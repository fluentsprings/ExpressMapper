using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class FatherSons
    {
        public int Id { get; set; }

        public int MyInt { get; set; }
        public string MyString { get; set; }

        public ICollection<Son> Sons { get; set; }

        public static FatherSons CreateOne(int count = 1, int numSons = 5)
        {
            return new FatherSons
            {
                MyInt = count,
                MyString = "Father",
                Sons = Son.CreateMany(count, numSons).ToList()
            };
        }
    }
}