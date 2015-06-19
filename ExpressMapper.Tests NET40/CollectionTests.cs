using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Tests.Model.Generator;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;
using NUnit.Framework;

namespace ExpressMapper.Tests
{
    [TestFixture]
    public class CollectionTests : BaseTestClass
    {
        [Test]
        public void AutoMemberMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var testData = Functional.CollectionAutoMemberMap();

            var result = testData.Key.MapTo<List<TestModel>, List<TestViewModel>>(null);

            Assert.AreEqual(result.Count, testData.Value.Count);

            for (var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void EnumerationToListTypeMap()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Compile();

            var testData = Functional.EnumerableToListTypeMap();

            var result = testData.Key.MapTo<IEnumerable<TestCollection>, List<TestCollectionViewModel>>();

            Assert.AreEqual(result.Count(), testData.Value.Count);

            for (var i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void EnumerationToArrayTypeMap()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Compile();

            var testData = Functional.EnumerableToArrayTypeMap();

            var result = testData.Key.MapTo<IEnumerable<TestCollection>, List<TestCollectionViewModel>>();

            Assert.AreEqual(result.Count(), testData.Value.Length);

            for (var i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void CustomMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.RegisterCustom<List<TestModel>, List<TestViewModel>, TestMapper>();
            Mapper.Compile();

            var testData = Functional.CollectionAutoMemberMap();

            var result = testData.Key.MapTo<List<TestModel>, List<TestViewModel>>();

            Assert.AreEqual(result.Count, testData.Value.Count);

            for (var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void NonGenericCollectionMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var testData = Functional.CollectionAutoMemberMap();

            var result = Mapper.Map(testData.Key, typeof(List<TestModel>), typeof(List<TestViewModel>)) as List<TestViewModel>;

            Assert.AreEqual(result.Count, testData.Value.Count);

            for (var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void NonGenericCollectionWithDestMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var testData = Functional.CollectionAutoMemberMap();

            var hashCode = testData.Value.GetHashCode();
            var itemHashes = testData.Value.Select(i => i.GetHashCode()).ToArray();

            var result = Mapper.Map(testData.Key, testData.Value, typeof(List<TestModel>), typeof(List<TestViewModel>)) as List<TestViewModel>;

            Assert.AreEqual(hashCode, result.GetHashCode());
            Assert.AreEqual(result.Count, testData.Value.Count);

            for (var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
                Assert.AreEqual(itemHashes[i], result[i].GetHashCode());
            }
        }
    }
}
