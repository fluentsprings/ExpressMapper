using ExpressMapper.Tests.Model.Generator;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;
using NUnit.Framework;

namespace ExpressMapper.Tests
{
    [TestFixture]
    public class ExceptionTests : BaseTestClass
    {
        [Test]
        public void NoMapping()
        {
            var exceptionMessage = string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", typeof(Size).FullName, typeof(SizeViewModel).FullName);
            Assert.Throws<MapNotImplemented>(() =>
            {
                var testData = Functional.NoMapping();
                testData.Key.MapTo<Size, SizeViewModel>();

            }, exceptionMessage);
        }

        [Test]
        public void NoMappingForProperty()
        {
            var exceptionMessage = string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", typeof(Country).FullName, typeof(Country).FullName);
            Assert.Throws<MapNotImplemented>(() =>
            {
                Mapper.Register<TestModel, TestViewModel>();
                Mapper.Register<Size, SizeViewModel>();
                Mapper.Compile();

                var testData = Functional.NoMappingForProperty();
                testData.Key.MapTo<TestModel, TestViewModel>();

            }, exceptionMessage);
        }
    }
}
