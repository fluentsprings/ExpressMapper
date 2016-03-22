using System.Linq;
using ExpressMapper.Extensions;
using ExpressMapper.Tests.Model.ViewModels;
using ExpressMapper.Tests.Projections.Entities;
using ExpressMapper.Tests.Projections.ViewModel;
using NUnit.Framework;

namespace ExpressMapper.Tests.Projections.Tests
{
    public class FlattenEntityTests : BaseTest
    {
        protected override void Setup()
        {
            Context.Set<Father>().Add(Father.CreateOne(1));         //Father->Son->Grandson

            var fatherWithoutGrandson = Father.CreateOne(2);
            fatherWithoutGrandson.Son.Grandson = null;
            Context.Set<Father>().Add(fatherWithoutGrandson);       //Father->Son

            Context.Set<FatherSons>().Add(FatherSons.CreateOne());  //FatherSons->5 Sons

            Context.SaveChanges();

            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().FlattenSource();
            Mapper.Register<FatherSons, FlattenFatherSonsCountDto>().FlattenSource();
            Mapper.Register<Father, FlattenFatherSonSimpleDto>().FlattenSource();
            Mapper.Register<Son, FlattenSimpleClass>();
            Mapper.Compile(CompilationTypes.Source);
        }

        protected override void Execute()
        {
            //no op
        }


        [Test]
        public void FlattenFatherSonGrandsonDtoOk()
        {
            //SETUP

            //ATTEMPT
            var results = Context.Set<Father>().Where(x => x.Son.Grandson != null).Project<Father, FlattenFatherSonGrandsonDto>().ToList();

            //VERIFY  
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Father", results.First().MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, results.First().MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", results.First().SonMyString);
            Assert.AreEqual(101, results.First().SonMyInt);
            Assert.AreEqual("Grandson", results.First().SonGrandsonMyString);
            Assert.AreEqual(10001, results.First().SonGrandsonMyInt);
        }


        [Test]
        public void FlattenFatherSonGrandsonDtoNoGrandsonOk()
        {
            //SETUP

            //ATTEMPT
            var results = Context.Set<Father>().Where(x => x.Son.Grandson == null).Project<Father, FlattenFatherSonGrandsonDto>().ToList();

            //VERIFY  
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Father", results.First().MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(2, results.First().MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", results.First().SonMyString);
            Assert.AreEqual(102, results.First().SonMyInt);
            Assert.AreEqual(null, results.First().SonGrandsonMyString);
            Assert.AreEqual(null, results.First().SonGrandsonMyInt);
        }

        [Test]
        public void FlattenFatherSonSimpleDtoOk()
        {
            //SETUP

            //ATTEMPT
            var results = Context.Set<Father>().Where(x => x.Son.Grandson != null).Project<Father, FlattenFatherSonSimpleDto>().ToList();

            //VERIFY   
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Father", results.First().MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, results.First().MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual("Son", results.First().Son.MyString);
            Assert.AreEqual(101, results.First().Son.MyInt);
        }

        [Test]
        public void FlattenFatherSonsCountDtoOk()
        {
            //SETUP

            //ATTEMPT
            var results = Context.Set<FatherSons>().Project<FatherSons, FlattenFatherSonsCountDto>().ToList();

            //VERIFY  
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Father", results.First().MyString);    //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(1, results.First().MyInt);              //This is mapped by the normal ExpressMapper 
            Assert.AreEqual(5, results.First().SonsCount);
        }

    }
}