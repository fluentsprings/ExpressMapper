namespace ExpressMapper.Tests.Model.Models
{
    public class FatherLowerCase
    {

        public int myint { get; set; }
        public string mystring { get; set; }

        public Son son { get; set; }

        public static FatherLowerCase CreateOne()
        {
            return new FatherLowerCase
            {
                myint = 1,
                mystring = "Father",
                son = Son.CreateOne()
            };
        }

    }
}