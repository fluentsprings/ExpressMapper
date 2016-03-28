using System.Linq;
using ExpressMapper.Extensions;
using ExpressMapper.Tests.Model.ViewModels;
using ExpressMapper.Tests.Projections.Entities;
using ExpressMapper.Tests.Projections.Utils;
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

            Mapper.Register<Father, FlattenFatherSonGrandsonDto>().Flatten();
            Mapper.Register<FatherSons, FlattenFatherSonsCountDto>().Flatten();
            Mapper.Register<Grandson, FlattenSimpleClass>();
            Mapper.Register<Father, FlattenFatherSonDtoForGrandsonDto>()
                .Flatten();
            Mapper.Compile(CompilationTypes.Source);
        }

        protected override void Execute()
        {
            //no op
        }


        [Test]
        public void FlattenFatherSonGrandsonDtoOk()
        {
            using (new LogDatabaseAccesses(Context))
            {                
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
        }


        [Test]
        public void FlattenFatherSonGrandsonDtoNoGrandsonOk()
        {
            using (new LogDatabaseAccesses(Context))
            { 
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
        }

        [Test]
        public void FlattenFatherSonDtoForGrandsonDtoOk()
        {
            using (new LogDatabaseAccesses(Context))
            {
                var results = Context.Set<Father>().Where(x => x.Son.Grandson != null).Project<Father, FlattenFatherSonDtoForGrandsonDto>().ToList();

                //VERIFY  
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual("Father", results.First().MyString);    //This is mapped by the normal ExpressMapper 
                Assert.AreEqual(1, results.First().MyInt);              //This is mapped by the normal ExpressMapper 
                Assert.AreEqual("Son", results.First().SonMyString);
                Assert.AreEqual(101, results.First().SonMyInt);
                Assert.AreEqual("Grandson", results.First().SonGrandson.MyString);
                Assert.AreEqual(10001, results.First().SonGrandson.MyInt);
            }
        }

        [Test]
        public void FlattenFatherSonDtoForGrandsonDtoNoGrandsonOk()
        {
            using (new LogDatabaseAccesses(Context))
            {
                var results = Context.Set<Father>().Where(x => x.Son.Grandson == null).Project<Father, FlattenFatherSonDtoForGrandsonDto>().ToList();

                //VERIFY  
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual("Father", results.First().MyString);    //This is mapped by the normal ExpressMapper 
                Assert.AreEqual(2, results.First().MyInt);              //This is mapped by the normal ExpressMapper 
                Assert.AreEqual("Son", results.First().SonMyString);
                Assert.AreEqual(102, results.First().SonMyInt);
                Assert.AreEqual(null, results.First().SonGrandson);
            }
        }


        [Test]
        public void FlattenFatherSonsCountDtoOk()
        {
            using (new LogDatabaseAccesses(Context))
            {
                var results = Context.Set<FatherSons>().Project<FatherSons, FlattenFatherSonsCountDto>().ToList();

                //VERIFY  
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual("Father", results.First().MyString); //This is mapped by the normal ExpressMapper 
                Assert.AreEqual(1, results.First().MyInt); //This is mapped by the normal ExpressMapper 
                Assert.AreEqual(5, results.First().SonsCount);
            }
        }

    }
}