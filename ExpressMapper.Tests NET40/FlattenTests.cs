using System;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;
using NUnit.Framework;

namespace ExpressMapper.Tests
{
    [TestFixture]
    public class FlattenTests : BaseTestClass
    {
        [Test]
        public void FlattenFatherSonGrandsonDtoOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var father = Father.CreateOne();
            var dto = Mapper.Map<Father, FlattenFatherSonGrandsonDto>(father);

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual(2, dto.SonMyInt);
            Assert.AreEqual("Grandson", dto.SonGrandsonMyString);
            Assert.AreEqual(3, dto.SonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonGrandsonDtoWithDestinationOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Destination);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonDto
            {
                MyInt = 29387465,
                MyString = "whjqegfqwjehfg"
            };
            var father = Father.CreateOne();
            Mapper.Map(father, dto);

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual(2, dto.SonMyInt);
            Assert.AreEqual("Grandson", dto.SonGrandsonMyString);
            Assert.AreEqual(3, dto.SonGrandsonMyInt);
        }

        [Test]
        public void FatherLowerCaseFlattenFatherSonGrandsonDtoOk()
        {
            //SETUP
            Mapper.Register<FatherLowerCase, FlattenFatherSonGrandsonDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonDto();
            Mapper.Map(FatherLowerCase.CreateOne(), dto);

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual(2, dto.SonMyInt);
            Assert.AreEqual("Grandson", dto.SonGrandsonMyString);
            Assert.AreEqual(3, dto.SonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonDtoForGrandsonDtoOk()
        {
            //SETUP
            Mapper.Register<Grandson, FlattenSimpleClass>();
            Mapper.Register<Father, FlattenFatherSonDtoForGrandsonDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var father = Father.CreateOne();
            var dto = Mapper.Map<Father, FlattenFatherSonDtoForGrandsonDto>(father);

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual(2, dto.SonMyInt);
            Assert.AreEqual("Grandson", dto.SonGrandson.MyString);
            Assert.AreEqual(3, dto.SonGrandson.MyInt);
        }

        [Test]
        public void FlattenFatherSonGrandsonLowerCaseDtoOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonLowerCaseDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var father = Father.CreateOne();
            var dto = Mapper.Map<Father, FlattenFatherSonGrandsonLowerCaseDto>(father);

            //VERIFY   
            Assert.AreEqual("Father", dto.mystring);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, dto.myInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", dto.sonMyString);
            Assert.AreEqual(2, dto.Sonmyint);
            Assert.AreEqual("Grandson", dto.SongrandsonmYstring);
            Assert.AreEqual(3, dto.sonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonGrandsonDtoCaseSensativeOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().CaseSensitive(true)
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var father = Father.CreateOne();
            var dto = Mapper.Map<Father, FlattenFatherSonGrandsonDto>(father);

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual(2, dto.SonMyInt);
            Assert.AreEqual("Grandson", dto.SonGrandsonMyString);
            Assert.AreEqual(3, dto.SonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonGrandsonLowerCaseDtoCaseSensativeOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonLowerCaseDto>()
                .CaseSensitive(true)
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = Mapper.Map<Father, FlattenFatherSonGrandsonLowerCaseDto>(Father.CreateOne());

            //VERIFY   
            Assert.AreEqual(null, dto.mystring);        //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(0, dto.myInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(null, dto.sonMyString);
            Assert.AreEqual(0, dto.Sonmyint);
            Assert.AreEqual(null, dto.SongrandsonmYstring);
            Assert.AreEqual(null, dto.sonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonGrandsonDtoNoSon()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var src = new Father
            {
                MyString = "Father",
                MyInt = 1
            };

            var dto = Mapper.Map<Father, FlattenFatherSonGrandsonDto>(src);

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, dto.MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(null, dto.SonMyString);
            Assert.AreEqual(0, dto.SonMyInt);
            Assert.AreEqual(null, dto.SonGrandsonMyString);
            Assert.AreEqual(null, dto.SonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonGrandsonDtoOverrideSonGrandsonMyStringOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Member(dest => dest.SonGrandsonMyString, src => src.MyString)
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var father = Father.CreateOne();
            var dto = Mapper.Map<Father, FlattenFatherSonGrandsonDto>(father);

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual("Father", dto.SonGrandsonMyString);
        }

        [Test]
        public void FlattenFatherSonGrandsonDtoIgnoreSonMyStringOk()
        {
            //SETUP
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Ignore(dest => dest.SonMyString)
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = Mapper.Map<Father, FlattenFatherSonGrandsonDto>(Father.CreateOne());

            //VERIFY   
            Assert.AreEqual("Father", dto.MyString);
            Assert.AreEqual(null, dto.SonMyString);
            Assert.AreEqual("Grandson", dto.SonGrandsonMyString);
        }

        [Test]
        public void FlattenFatherSonsCountDtoOk()
        {
            //SETUP
            Mapper.Register<FatherSons, FlattenFatherSonsCountDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var fatherSons = FatherSons.CreateOne();
            var dto = Mapper.Map<FatherSons, FlattenFatherSonsCountDto>(fatherSons);

            //VERIFY  
            Assert.AreEqual("Father", dto.MyString);
            Assert.AreEqual(5, dto.SonsCount);
        }

        [Test]
        public void FlattenLinqCollectionMethodsDtoOk()
        {
            //SETUP
            Mapper.Register<FatherSons, FlattenLinqCollectionMethodsDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = Mapper.Map<FatherSons, FlattenLinqCollectionMethodsDto>(FatherSons.CreateOne());

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
            Mapper.Register<FlattenCircularReference, FlattenCircularReferenceDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = Mapper.Map<FlattenCircularReference, FlattenCircularReferenceDto>(FlattenCircularReference.CreateOne());

            //VERIFY  
            Assert.AreEqual("Outer", dto.MyString);
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual("Inner", dto.CircularRefMyString);
        }
    }
}