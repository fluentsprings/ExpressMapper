using System;
using System.Collections.Generic;
using Benchmarks.Generators;
using Benchmarks.Mapping;
using Benchmarks.Models;
using Benchmarks.ViewModels;
using Mapster;

namespace Benchmarks.Tests
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

        protected override List<AuthorViewModel> AutoMapperMap(List<Author> src)
        {
            return AutoMapper.Mapper.Map<List<Author>, List<AuthorViewModel>>(src);
        }

        protected override List<AuthorViewModel> ExpressMapperMap(List<Author> src)
        {
            return ExpressMapper.Mapper.Map<List<Author>, List<AuthorViewModel>>(src);
        }

        protected override List<AuthorViewModel> OoMapperMap(List<Author> src)
        {
            var authorViewModels = OoMapper.Mapper.Map<List<Author>, List<AuthorViewModel>>(src);
            return authorViewModels;
        }

        protected override List<AuthorViewModel> ValueInjectorMap(List<Author> src)
        {
            var list = new List<AuthorViewModel>();
            foreach (var item in src)
            {
                list.Add(Omu.ValueInjecter.Mapper.Map<Author, AuthorViewModel>(item));
            }
            return list;
        }

        protected override List<AuthorViewModel> MapsterMap(List<Author> src)
        {
            var authorViewModels = TypeAdapter.Adapt<List<Author>, List<AuthorViewModel>>(src);
            return authorViewModels;
        }

        protected override List<AuthorViewModel> TinyMapperMap(List<Author> src)
        {
            // custom mapping is not supported
            throw new NotImplementedException();
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

        protected override string Size
        {
            get { return "L"; }
        }
    }
}
