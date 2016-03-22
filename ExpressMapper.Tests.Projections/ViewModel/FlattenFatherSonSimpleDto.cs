using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class FlattenFatherSonSimpleDto
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public FlattenSimpleClass Son { get; set; }
    }
}