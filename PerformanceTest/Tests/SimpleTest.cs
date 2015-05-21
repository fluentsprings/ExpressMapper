using System.Collections.Generic;
using Mapster;
using Nelibur.ObjectMapper;
using PerformanceTest.Generators;
using PerformanceTest.Mapping;
using PerformanceTest.Models;
using PerformanceTest.ViewModels;

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

        protected override void InitOoMapper()
        {
            OoMapperMappings.Init();
        }

        protected override void InitValueInjectorMapper()
        {
            ValueInjectorMappings.Init();
        }

        protected override void InitMapsterMapper()
        {
            MapsterMapperMappings.Init();
        }

        protected override void InitTinyMapper()
        {
            TinyMapperMappings.Init();
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

        protected override List<NewsViewModel> OoMapperMap(List<News> src)
        {
            return OoMapper.Mapper.Map<List<News>, List<NewsViewModel>>(src);
        }

        protected override List<NewsViewModel> ValueInjectorMap(List<News> src)
        {
            var list = new List<NewsViewModel>();
            foreach (var item in src)
            {
                list.Add(Omu.ValueInjecter.Mapper.Map<News, NewsViewModel>(item));
            }
            return list;
        }

        protected override List<NewsViewModel> MapsterMap(List<News> src)
        {
            return TypeAdapter.Adapt<List<News>, List<NewsViewModel>>(src);
        }

        protected override List<NewsViewModel> TinyMapperMap(List<News> src)
        {
            var list = new List<NewsViewModel>();
            foreach (var item in src)
            {
                list.Add(TinyMapper.Map<News, NewsViewModel>(item));
            }
            return list;
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

        protected override string Size
        {
            get { return "S"; }
        }
    }
}
