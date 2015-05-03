using System.Collections.Generic;
using ExpressMapper.Tests.Models;
using ExpressMapper.Tests.ViewModels;

namespace ExpressMapper.Tests.CustomMappers
{
    public class TestMapper : ICustomTypeMapper<List<TestModel>, List<TestViewModel>>
    {
        public List<TestViewModel> Map(List<TestModel> src)
        {
            var testViewModels = new List<TestViewModel>();
            foreach (var testModel in src)
            {
                testViewModels.Add(testModel.MapTo<TestModel, TestViewModel>());
            }
            return testViewModels;
        }
    }
}
