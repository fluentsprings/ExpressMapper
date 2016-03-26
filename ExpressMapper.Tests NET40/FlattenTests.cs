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
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonDto();
            Mapper.Map(Father.CreateOne(), dto);

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
            Mapper.Reset();
            Mapper.Register<FatherLowerCase, FlattenFatherSonGrandsonDto>().FlattenSource();
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

        [Ignore("This does not work - awaiting decision on ExpressMapping inside Cutomer members")]
        [Test]
        public void FlattenFatherSonDtoForGrandsonDtoOk()
        {
            //SETUP
            Mapper.Reset();
            Mapper.Register<Grandson, FlattenSimpleClass>();
            Mapper.Register<Father, FlattenFatherSonDtoForGrandsonDto>() //.FlattenSource();
                .Member(dest => dest.SonMyInt, src => src.Son.MyInt)
                .Member(dest => dest.SonMyString, src => src.Son.MyString)
                .Member(dest => dest.SonGrandson, src => src.Son.Grandson);
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonDtoForGrandsonDto();
            Mapper.Map(Father.CreateOne(), dto);

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
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonLowerCaseDto>().FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonLowerCaseDto();
            Mapper.Map(Father.CreateOne(), dto);

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
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().CaseSensitive(true).FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonDto();
            Mapper.Map(Father.CreateOne(), dto);

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
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonLowerCaseDto>().CaseSensitive(true).FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonLowerCaseDto();
            Mapper.Map(Father.CreateOne(), dto);

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
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonDto();
            var src = new Father
            {
                MyString = "Father",
                MyInt = 1
            };
            Mapper.Map(src, dto);

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
            Mapper.Reset();
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>()
                .Member(dest => dest.SonGrandsonMyString, src => src.MyString).FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenFatherSonGrandsonDto();
            Mapper.Map(Father.CreateOne(), dto);

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
            var dto = new FlattenFatherSonGrandsonDto();
            Mapper.Map(Father.CreateOne(), dto);

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
            var dto = new FlattenFatherSonsCountDto();
            Mapper.Map(FatherSons.CreateOne(), dto);

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
            var dto = new FlattenLinqCollectionMethodsDto();
            Mapper.Map(FatherSons.CreateOne(), dto);

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
            Mapper.Register<FlattenCircularReference, FlattenCircularReferenceDto>().FlattenSource();
            Mapper.Compile(CompilationTypes.Source);

            //ATTEMPT
            var dto = new FlattenCircularReferenceDto();
            Mapper.Map(FlattenCircularReference.CreateOne(), dto);

            //VERIFY  
            Assert.AreEqual("Outer", dto.MyString);
            Assert.AreEqual("Son", dto.SonMyString);
            Assert.AreEqual("Inner", dto.CircularRefMyString);
        }

        //----------------------------------
        //Failure tests

        [Test]
        public void FlattenFatherSonsCountBadDtoOk()
        {
            //SETUP

            //ATTEMPT
            var ex = Assert.Throws<InvalidOperationException>( () => Mapper.Register<FatherSons, FlattenFatherSonsCountBadDto>().FlattenSource());

            //VERIFY  
            Assert.AreEqual("We could not find the Method Count() which matched the property SonsCount of type System.String.", ex.Message);
        }
    }
}