using System.Collections.Generic;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests
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
