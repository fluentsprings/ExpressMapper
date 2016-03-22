using System.Threading;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class Grandson
    {
        public int Id { get; set; }
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public static Grandson CreateOne(int count)
        {
            return new Grandson
            {
                MyInt = 10000 + count,
                MyString = "Grandson"
            };
        }
    }
}