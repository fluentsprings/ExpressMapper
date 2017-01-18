using System;
using System.Collections.Generic;
using ExpressMapper.Tests.Model.Generator;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;
using NUnit.Framework;

namespace ExpressMapper.Tests
{
    [TestFixture]
    public class ExceptionTests : BaseTestClass
    {
        //[Test]
        //public void MappingRegisteredMoreThanOnceTest()
        //{
        //    Mapper.Register<Size, SizeViewModel>();

        //    var exceptionMessage = string.Format("Mapping from {0} to {1} is already registered",
        //        typeof (Size).FullName, typeof (SizeViewModel).FullName);
        //    Assert.Throws<InvalidOperationException>(() =>
        //    {
        //        Mapper.Register<Size, SizeViewModel>();

        //    }, exceptionMessage);
        //}

        [Test]
        public void RegisteringCollectionTypesTest()
        {
            var exceptionMessage =
                string.Format(
                    "It is invalid to register mapping for collection types from {0} to {1}, please use just class registration mapping and your collections will be implicitly processed. In case you want to include some custom collection mapping please use: Mapper.RegisterCustom.",
                    typeof(List<Size>).FullName, typeof(SizeViewModel[]).FullName);
            Assert.Throws<InvalidOperationException>(() =>
            {
                Mapper.Register<List<Size>, SizeViewModel[]>();
            }, exceptionMessage);
        }
    }
}
