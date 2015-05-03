using System.Collections.Generic;
using PerformanceTest.Generators;
using PerformanceTest.Mapping;
using PerformanceTest.Models;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Tests
{
    public class SimpleWithAssociationTest : BaseTest<List<User>, List<UserViewModel>>
    {
        protected override List<User> GetData()
        {
            return DataGenerator.GetUsers(Count);
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

        protected override List<UserViewModel> AutoMapperMap(List<User> src)
        {
            return AutoMapper.Mapper.Map<List<User>, List<UserViewModel>>(src);
        }

        protected override List<UserViewModel> ExpressMapperMap(List<User> src)
        {
            return ExpressMapper.Mapper.Map<List<User>, List<UserViewModel>>(src);
        }

        protected override List<UserViewModel> NativeMapperMap(List<User> src)
        {
            var result = new List<UserViewModel>();
            foreach (var user in src)
            {
                result.Add(NativeMapping.Map(user));
            }
            return result;
        }

        protected override string TestName
        {
            get { return "SimpleWithAssociationTest"; }
        }
    }
}
