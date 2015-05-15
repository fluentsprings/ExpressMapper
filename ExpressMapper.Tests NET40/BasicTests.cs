using System.Linq;
using ExpressMapper.Tests.Model.Enums;
using ExpressMapper.Tests.Model.Generator;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.Models.Structs;
using ExpressMapper.Tests.Model.ViewModels;
using ExpressMapper.Tests.Model.ViewModels.Structs;
using NUnit.Framework;

namespace ExpressMapper.Tests
{
    [TestFixture]
    public class BasicTests : BaseTestClass
    {
        [Test]
        public void AutoMemberMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var test = Functional.AutoMemberMap();

            var testViewModel = test.Key.MapTo<TestModel, TestViewModel>();

            Assert.AreEqual(testViewModel, test.Value);
        }

        [Test]
        public void ManualPrimitiveMemberMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Member(src => src.Name, dest => string.Format("Full - {0} - Size", dest.Alias))
                .Member(src => src.SortOrder, dest => dest.Id.GetHashCode());
            Mapper.Compile();

            var sizeResult = Functional.ManualPrimitiveMemberMap();

            var result = sizeResult.Key.MapTo<Size, SizeViewModel>();

            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void InstantiateMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Instantiate(src => new SizeViewModel(s => string.Format("{0} - Full name - {1}", src.Id, s)));
            Mapper.Compile();

            var sizeResult = Functional.InstantiateMap();

            var result = sizeResult.Key.MapTo<Size, SizeViewModel>();

            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void IgnoreMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Ignore(dest => dest.Name);
            Mapper.Compile();

            var sizeResult = Functional.IgnoreMap();
            var result = sizeResult.Key.MapTo<Size, SizeViewModel>();
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void BeforeMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Before((src, dest) => dest.Name = src.Name)
                .Ignore(dest => dest.Name);
            Mapper.Compile();

            var sizeResult = Functional.BeforeMap();
            var result = sizeResult.Key.MapTo<Size, SizeViewModel>();
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void AfterMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .After((src, dest) => dest.Name = "OVERRIDE BY AFTER MAP");
            Mapper.Compile();
            var sizeResult = Functional.AfterMap();
            var result = sizeResult.Key.MapTo<Size, SizeViewModel>();
            Assert.AreEqual(result, sizeResult.Value);
        }


        [Test]
        public void CustomMapWithSupportedCollectionMaps()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Custom(new SizeMapper());
            Mapper.Compile();
            var sizeResult = Functional.CustomMap();
            var result = sizeResult.Key.MapTo<Size, SizeViewModel>();
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void CustomMapWithSupportedNestedCollectionMaps()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Register<Size, SizeViewModel>()
                .Custom(new SizeMapper());
            Mapper.Compile();
            var sizeResult = Functional.CustomNestedCollectionMap();
            var result = sizeResult.Key.MapTo<TestModel, TestViewModel>();
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void NullPropertyAndNullCollectionPropertyMaps()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Register<Size, SizeViewModel>();
                
            Mapper.Compile();
            var sizeResult = Functional.NullPropertyAndNullCollectionMap();
            var result = sizeResult.Key.MapTo<TestModel, TestViewModel>();
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void OnlyGetPropertyMaps()
        {
            Mapper.Register<Supplier, SupplierViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Compile();
            var supplierResult = Functional.GetPropertyMaps();
            var result = supplierResult.Key.MapTo<Supplier, SupplierViewModel>();
            Assert.AreEqual(result, supplierResult.Value);
        }

        [Test]
        public void OnlyGetWithManualPropertyMaps()
        {
            Mapper.Register<Supplier, SupplierViewModel>()
                .Member(dest => dest.Rank, src => src.Rank);
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Compile();
            var supplierResult = Functional.GetPropertyMaps();
            var result = supplierResult.Key.MapTo<Supplier, SupplierViewModel>();
            Assert.AreEqual(result, supplierResult.Value);
        }

        [Test]
        public void CustomMap()
        {
            Mapper.RegisterCustom<GenderTypes, string>(g => g.ToString());
            Mapper.Compile();
                
            var result = GenderTypes.Men.MapTo<GenderTypes, string>();
            Assert.AreEqual(result, GenderTypes.Men.ToString());
        }

        [Test]
        public void AutoMemberStructMap()
        {
            Mapper.Register<Item, ItemViewModel>();
            Mapper.Compile();
            var testData = Functional.AutoMemberStructMap();

            var result = testData.Key.MapTo<Item, ItemViewModel>();
            Assert.AreEqual(result, testData.Value);
        }

        [Test]
        public void StructWithCollectionMap()
        {
            Mapper.Register<Feature, FeatureViewModel>();
            Mapper.Register<Item, ItemViewModel>()
                .Member(dest => dest.FeatureList, src => src.Features);
            Mapper.Compile();
            var testData = Functional.StructWithCollectionMap();

            var result = testData.Key.MapTo<Item, ItemViewModel>();
            Assert.AreEqual(result, testData.Value);
        }

        [Test]
        public void ComplexMap()
        {
            Mapper.Register<FashionProduct, FashionProductViewModel>()
                .Function(dest => dest.OptionalGender, src =>
                {
                    GenderTypes? optionalGender;
                    switch (src.Gender)
                    {
                            case GenderTypes.Unisex:
                            optionalGender = null;
                            break;
                        default :
                            optionalGender = src.Gender;
                            break;
                    }
                    return optionalGender;
                });
            Mapper.Register<ProductOption, ProductOptionViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Feature, FeatureViewModel>();
            Mapper.Register<City, CityViewModel>()
                .Member(dest=> dest.FeaturesList, src => src.Features);
            Mapper.Register<Supplier, SupplierViewModel>();
            Mapper.Register<Brand, BrandViewModel>();
            
            Mapper.Compile();
            var testData = Functional.ComplexMap();

            var result = testData.Key.MapTo<FashionProduct, FashionProductViewModel>();
            var valid = result.Equals(testData.Value);
            Assert.IsTrue(valid);
        }

        [Test]
        public void ListToArray()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Array, src => src.List)
                .Ignore(dest => dest.Collection)
                .Ignore(dest => dest.Enumerable)
                .Ignore(dest => dest.List)
                .Ignore(dest => dest.Queryable);
            
            Mapper.Compile();

            var typeCollTest = Functional.CollectionTypeMap();
            var result = typeCollTest.Key.MapTo<TestItem, TestItemViewModel>();
            Assert.AreEqual(result.Array.Length, typeCollTest.Key.List.Count);
        }

        [Test]
        public void ListToQueriable()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Queryable, src => src.List)
                .Ignore(dest => dest.Collection)
                .Ignore(dest => dest.List)
                .Ignore(dest => dest.Array)
                .Ignore(dest => dest.Enumerable);

            Mapper.Compile();

            var typeCollTest = Functional.CollectionTypeMap();
            var result = typeCollTest.Key.MapTo<TestItem, TestItemViewModel>();
            Assert.AreEqual(result.Queryable.Count(), typeCollTest.Key.List.Count());
        }

        [Test]
        public void EnumerableToQueriable()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Queryable, src => src.Enumerable)
                .Ignore(dest => dest.Collection)
                .Ignore(dest => dest.List)
                .Ignore(dest => dest.Array)
                .Ignore(dest => dest.Enumerable);

            Mapper.Compile();

            var typeCollTest = Functional.CollectionTypeMap();
            var result = typeCollTest.Key.MapTo<TestItem, TestItemViewModel>();
            Assert.AreEqual(result.Queryable.Count(), typeCollTest.Key.Enumerable.Count());
        }

        [Test]
        public void QueriableToArray()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Array, src => src.Queryable)
                .Ignore(dest => dest.Collection)
                .Ignore(dest => dest.List)
                .Ignore(dest => dest.Queryable)
                .Ignore(dest => dest.Enumerable);

            Mapper.Compile();

            var typeCollTest = Functional.CollectionTypeMap();
            var result = typeCollTest.Key.MapTo<TestItem, TestItemViewModel>();
            Assert.AreEqual(result.Array.Count(), typeCollTest.Key.Queryable.Count());
        }
    }
}
