using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Extensions;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;
using NUnit.Framework;

namespace ExpressMapper.Tests
{
    [TestFixture]
    public class FlattenProjectTests : BaseTestClass
    {

        [Test]
        public void FlattenFatherSonsCountDtoOk()
        {
            //SETUP
            Mapper.Register<FatherSons, FlattenFatherSonsCountDto>().Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = FatherSons.CreateOne();
            var queryData = new List<FatherSons> { single }.AsQueryable();
            var dto = queryData.Project<FatherSons, FlattenFatherSonsCountDto>().Single();

            //VERIFY  
            Assert.AreEqual("Father", dto.MyString);
            Assert.AreEqual(5, dto.SonsCount);
        }

        [Test]
        public void FlattenLinqCollectionMethodsDtoOk()
        {
            //SETUP
            Mapper.Register<FatherSons, FlattenLinqCollectionMethodsDto>().Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = FatherSons.CreateOne();
            var queryData = new List<FatherSons> { single }.AsQueryable();
            var dto = queryData.Project<FatherSons, FlattenLinqCollectionMethodsDto>().Single();

            //VERIFY  
            Assert.AreEqual(true, dto.SonsAny);
            Assert.AreEqual(5, dto.SonsCount);
            Assert.AreEqual(5, dto.SonsLongCount);
            Assert.AreEqual("Son", dto.SonsFirstOrDefault.MyString);
        }

        [Test]
        public void FlattenCircularReferenceDtoOk()
        {
            //SETUP
            Mapper.Reset();
            Mapper.Register<FlattenCircularReference, FlattenCircularReferenceDto>().Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = FlattenCircularReference.CreateOne();
            var queryData = new List<FlattenCircularReference> { single }.AsQueryable();
            var dto = queryData.Project<FlattenCircularReference, FlattenCircularReferenceDto>().Single();

            //VERIFY  
            Assert.AreEqual("Outer", dto.MyString);
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual("Inner", dto.CircularRefMyString);
        }

    }
}