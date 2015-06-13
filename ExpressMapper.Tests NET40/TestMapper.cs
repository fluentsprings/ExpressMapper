using System.Collections.Generic;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests
{
    public class TestMapper : ICustomTypeMapper<List<TestModel>, List<TestViewModel>>
    {
        public List<TestViewModel> Map(IMappingContext<List<TestModel>, List<TestViewModel>> context)
        {
            var testViewModels = context.Destination ?? new List<TestViewModel>();
            foreach (var testModel in context.Source)
            {
                testViewModels.Add(testModel.MapTo<TestModel, TestViewModel>());
            }
            return testViewModels;
        }
    }
}
