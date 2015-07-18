using System;
using System.Collections.Generic;
using Benchmarks.Generators;
using Benchmarks.Mapping;
using Benchmarks.Models;
using Benchmarks.ViewModels;
using Mapster;

namespace Benchmarks.Tests
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

        protected override List<UserViewModel> AutoMapperMap(List<User> src)
        {
            return AutoMapper.Mapper.Map<List<User>, List<UserViewModel>>(src);
        }

        protected override List<UserViewModel> ExpressMapperMap(List<User> src)
        {
            return ExpressMapper.Mapper.Map<List<User>, List<UserViewModel>>(src);
        }

        protected override List<UserViewModel> OoMapperMap(List<User> src)
        {
            var userViewModels = OoMapper.Mapper.Map<List<User>, List<UserViewModel>>(src);
            return userViewModels;
        }

        protected override List<UserViewModel> ValueInjectorMap(List<User> src)
        {
            var list = new List<UserViewModel>();
            foreach (var item in src)
            {
                list.Add(Omu.ValueInjecter.Mapper.Map<User, UserViewModel>(item));
            }
            return list;
        }

        protected override List<UserViewModel> MapsterMap(List<User> src)
        {
            var userViewModels = TypeAdapter.Adapt<List<User>, List<UserViewModel>>(src);
            return userViewModels;
        }

        protected override List<UserViewModel> TinyMapperMap(List<User> src)
        {
            // custom mapping is not supported
            throw new NotImplementedException();
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

        protected override string Size
        {
            get { return "M"; }
        }
    }
}
