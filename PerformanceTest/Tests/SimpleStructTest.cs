using System.Collections.Generic;
using PerformanceTest.Generators;
using PerformanceTest.Mapping;
using PerformanceTest.Models;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Tests
{
    public class SimpleStructTest : BaseTest<List<Item>, List<ItemViewModel>>
    {
        protected override List<Item> GetData()
        {

            return DataGenerator.GetItems(Count);
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

        protected override List<ItemViewModel> AutoMapperMap(List<Item> src)
        {
            return AutoMapper.Mapper.Map<List<Item>, List<ItemViewModel>>(src);
        }

        protected override List<ItemViewModel> ExpressMapperMap(List<Item> src)
        {
            return ExpressMapper.Mapper.Map<List<Item>, List<ItemViewModel>>(src);
        }

        protected override List<ItemViewModel> NativeMapperMap(List<Item> src)
        {
            var result = new List<ItemViewModel>();
            foreach (var item in src)
            {
                result.Add(NativeMapping.Map(item));
            }
            return result;
        }

        protected override string TestName
        {
            get { return "SimpleStructTest"; }
        }
    }
}
