using System.Collections.Generic;
using PerformanceTest.Generators;
using PerformanceTest.Mapping;
using PerformanceTest.Models;

namespace PerformanceTest.Tests
{
    public class SimpleTest : BaseTest<List<News>, List<NewsViewModel>>
    {
        protected override List<News> GetData()
        {
            return DataGenerator.GetNews(Count);
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

        protected override List<NewsViewModel> AutoMapperMap(List<News> src)
        {
            return AutoMapper.Mapper.Map<List<News>, List<NewsViewModel>>(src);
        }

        protected override List<NewsViewModel> ExpressMapperMap(List<News> src)
        {
            return ExpressMapper.Mapper.Map<List<News>, List<NewsViewModel>>(src);
        }

        protected override List<NewsViewModel> NativeMapperMap(List<News> src)
        {
            var result = new List<NewsViewModel>();
            foreach (var newse in src)
            {
                result.Add(NativeMapping.Map(newse));
            }
            return result;
        }

        protected override string TestName
        {
            get { return "SimpleTest"; }
        }
    }
}
