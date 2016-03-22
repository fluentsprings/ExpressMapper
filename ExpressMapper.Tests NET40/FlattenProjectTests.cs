using System;
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
        public void FlattenFatherSonGrandsonDtoOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = Father.CreateOne();
            var queryData = new List<Father> { single }.AsQueryable();
            var dto = queryData.Project<Father, FlattenFatherSonGrandsonDto>().Single();

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal AutoMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal AutoMapper 
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual(2, dto.SonMyInt);
            Assert.AreEqual("Grandson", dto.SonGrandsonMyString);
            Assert.AreEqual(3, dto.SonGrandsonMyInt);
        }


        [Test]
        public void FlattenFatherSonGrandsonDtoNoGrandson()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = Father.CreateOne();
            single.Son.Grandson = null;
            var queryData = new List<Father> { single }.AsQueryable();
            var dto = queryData.Project<Father, FlattenFatherSonGrandsonDto>().Single();

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal AutoMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal AutoMapper 
            Assert.AreEqual(null, dto.SonMyString);
            Assert.AreEqual(0, dto.SonMyInt);
            Assert.AreEqual(null, dto.SonGrandsonMyString);
            Assert.AreEqual(0, dto.SonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonGrandsonDtoOverrideSonGrandsonMyStringOk()
        {
            //SETUP
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Member(dest => dest.SonGrandsonMyString, src => src.MyString).FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = Father.CreateOne();
            var queryData = new List<Father> { single }.AsQueryable();
            var dto = queryData.Project<Father, FlattenFatherSonGrandsonDto>().Single();

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual("Father", dto.SonGrandsonMyString);
        }

        [Test]
        public void FlattenFatherSonGrandsonDtoIgnoreSonMyStringOk()
        {
            //SETUP
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Ignore(dest => dest.SonMyString).FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = Father.CreateOne();
            var queryData = new List<Father> { single }.AsQueryable();
            var dto = queryData.Project<Father, FlattenFatherSonGrandsonDto>().Single();

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);
            Assert.AreEqual(null, dto.SonMyString);
            Assert.AreEqual("Grandson", dto.SonGrandsonMyString);
        }

        [Test]
        public void FlattenFatherSonsCountDtoOk()
        {
            //SETUP
            Mapper.Reset();
            Mapper.Register<FatherSons, FlattenFatherSonsCountDto>().FlattenSource();
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
            Mapper.Reset();
            Mapper.Register<FatherSons, FlattenLinqCollectionMethodsDto>().FlattenSource();
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
            Mapper.Register<FlattenCircularReference, FlattenCircularReferenceDto>() //.FlattenSource();
                .Member(dest => dest.SonMyString, src => src.Son.MyString)
                .Member(dest => dest.CircularRefMyString, src => src.CircularRef.MyString);
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var single = new FlattenCircularReference();
            var queryData = new List<FlattenCircularReference> { single }.AsQueryable();
            var dto = queryData.Project<FlattenCircularReference, FlattenCircularReferenceDto>().Single();

            //VERIFY  
            Assert.AreEqual("Outer", dto.MyString);
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual("Inner", dto.CircularRefMyString);
        }

    }
}