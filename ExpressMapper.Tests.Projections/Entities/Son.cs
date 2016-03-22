using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class Son
    {
        public int Id { get; set; }

        public int MyInt { get; set; }
        public string MyString { get; set; }

        public Grandson Grandson { get; set; }

        public static Son CreateOne(int count)
        {
            return new Son
            {
                MyInt = 100 + count,
                MyString = "Son",
                Grandson = Grandson.CreateOne(count)
            };
        }

        public static IEnumerable<Son> CreateMany(int count, int num = 5)
        {
            for (int i = 0; i < num; i++)
            {
                yield return CreateOne(count + i);
            }
        }

    }
}