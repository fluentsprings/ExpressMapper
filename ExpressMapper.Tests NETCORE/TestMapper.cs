using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;
using System.Collections.Generic;

namespace ExpressMapper.Tests
{
    public class TestMapper : ICustomTypeMapper<List<TestModel>, List<TestViewModel>>
    {
        public List<TestViewModel> Map(IMappingContext<List<TestModel>, List<TestViewModel>> context)
        {
            var testViewModels = context.Destination ?? new List<TestViewModel>();
            foreach (var testModel in context.Source)
            {
                testViewModels.Add(Mapper.Map<TestModel, TestViewModel>(testModel));
            }
            return testViewModels;
        }
    }
}
