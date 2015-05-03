using System.Collections.Generic;
using PerformanceTest.Generators;
using PerformanceTest.Mapping;
using PerformanceTest.Models;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Tests
{
    public class ComplexTest : BaseTest<List<Test>, List<TestViewModel>>
    {
        protected override List<Test> GetData()
        {
            return DataGenerator.GetTests(Count);
        }

        protected override void InitAutoMapper()
        {
            AutoMapperMapping.Init();
        }

        protected override void InitExpressMapper()
        {
            ExpressMapperMapping.Init();
        }

        protected override void InitNativeMapper()
        {
        }

        protected override List<TestViewModel> AutoMapperMap(List<Test> src)
        {
            return AutoMapper.Mapper.Map<List<Test>, List<TestViewModel>>(src);
        }

        protected override List<TestViewModel> ExpressMapperMap(List<Test> src)
        {
            return ExpressMapper.Mapper.Map<List<Test>, List<TestViewModel>>(src);
        }

        protected override List<TestViewModel> NativeMapperMap(List<Test> src)
        {
            var list = new List<TestViewModel>();
            foreach (var test in src)
            {
                list.Add(NativeMapping.Map(test));
            }
            return list;
        }

        protected override string TestName
        {
            get { return "ComplexTest"; }
        }
    }
}
