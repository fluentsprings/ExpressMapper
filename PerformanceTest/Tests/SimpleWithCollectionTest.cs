using System.Collections.Generic;
using PerformanceTest.Generators;
using PerformanceTest.Mapping;
using PerformanceTest.Models;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Tests
{
    public class SimpleWithCollectionTest : BaseTest<List<Author>, List<AuthorViewModel>>
    {
        protected override List<Author> GetData()
        {
            return DataGenerator.GetAuthors(Count);
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

        protected override List<AuthorViewModel> AutoMapperMap(List<Author> src)
        {
            return AutoMapper.Mapper.Map<List<Author>, List<AuthorViewModel>>(src);
        }

        protected override List<AuthorViewModel> ExpressMapperMap(List<Author> src)
        {
            return ExpressMapper.Mapper.Map<List<Author>, List<AuthorViewModel>>(src);
        }

        protected override List<AuthorViewModel> NativeMapperMap(List<Author> src)
        {
            var result = new List<AuthorViewModel>();
            foreach (var author in src)
            {
                result.Add(NativeMapping.Map(author));
            }
            return result;
        }

        protected override string TestName
        {
            get { return "SimpleWithCollectionTest"; }
        }
    }
}
