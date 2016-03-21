using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.Models
{
    public class FatherSons
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public ICollection<Son> Sons { get; set; }

        public static FatherSons CreateOne(int numSons = 5, Random rand = null)
        {
            rand = rand ?? new Random();
            return new FatherSons
            {
                MyInt = 1,
                MyString = "Father",
                Sons = Son.CreateMany(numSons).ToList()
            };
        }

        public static IEnumerable<FatherSons> CreateMany(int num = 5, int numSons = 5)
        {
            var rand = new Random();
            for (int i = 0; i < num; i++)
            {
                yield return CreateOne(numSons, rand);
            }
        }
    }
}