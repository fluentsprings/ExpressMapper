using System;
using System.Collections.Generic;
using Benchmarks.Generators;
using Benchmarks.Mapping;
using Benchmarks.Models;
using Benchmarks.ViewModels;
using Mapster;

namespace Benchmarks.Tests
{
    public class AdvancedTest : BaseTest<List<Test>, List<TestViewModel>>
    {
        protected override List<Test> GetData()
        {
            return DataGenerator.GetTests(Count);
        }

        protected override void InitAutoMapper()
        {
            AutoMapperMapping.InitAdvanced();
        }

        protected override void InitExpressMapper()
        {
            ExpressMapperMapping.InitAdvanced();
        }

        protected override void InitOoMapper()
        {
            OoMapperMappings.InitAdvanced();
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

        protected override List<TestViewModel> AutoMapperMap(List<Test> src)
        {
            return AutoMapper.Mapper.Map<List<Test>, List<TestViewModel>>(src);
        }

        protected override List<TestViewModel> ExpressMapperMap(List<Test> src)
        {
            return ExpressMapper.Mapper.Map<List<Test>, List<TestViewModel>>(src);
        }

        protected override List<TestViewModel> OoMapperMap(List<Test> src)
        {
            var testViewModels = OoMapper.Mapper.Map<List<Test>, List<TestViewModel>>(src);
            return testViewModels;
        }

        protected override List<TestViewModel> ValueInjectorMap(List<Test> src)
        {
            var list = new List<TestViewModel>();
            foreach (var item in src)
            {
                list.Add(Omu.ValueInjecter.Mapper.Map<Test, TestViewModel>(item));
            }
            return list;
        }

        protected override List<TestViewModel> MapsterMap(List<Test> src)
        {
            var testViewModels = TypeAdapter.Adapt<List<Test>, List<TestViewModel>>(src);
            return testViewModels;
        }

        protected override List<TestViewModel> TinyMapperMap(List<Test> src)
        {
            throw new NotImplementedException();
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
            get { return "AdvancedTest"; }
        }

        protected override string Size
        {
            get { return "XL"; }
        }
    }
}
