namespace ExpressMapper.Tests.Model.Models
{
    public class FlattenCircularReference
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public Son Son { get; set; }

        public FlattenCircularReference CircularRef { get; set; }


        public static FlattenCircularReference CreateOne()
        {
            var outer = new FlattenCircularReference
            {
                MyInt = 1,
                MyString = "Outer",
                Son = Son.CreateOne()
            };
            outer.CircularRef = new FlattenCircularReference
            {
                MyInt = 2,
                MyString = "Inner",
                CircularRef = outer
            };
            return outer;
        }
    }
}