using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class FlattenFatherSonDtoForGrandsonDto
    {
        public int MyInt { get; set; }
        public string MyString { get; set; }

        public int SonMyInt { get; set; }
        public string SonMyString { get; set; }

        public FlattenSimpleClass SonGrandson { get; set; }
    }
}