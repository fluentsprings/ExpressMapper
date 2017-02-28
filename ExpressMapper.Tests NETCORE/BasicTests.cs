using ExpressMapper.Extensions;
using ExpressMapper.Tests.Model.Enums;
using ExpressMapper.Tests.Model.Generator;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.Models.Structs;
using ExpressMapper.Tests.Model.ViewModels;
using ExpressMapper.Tests.Model.ViewModels.Structs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
//using Moq;

namespace ExpressMapper.Tests
{
    [TestFixture]
    public class BasicTests : BaseTestClass
    {
        [Test]
        public void EnumToAnotherEnumMapTest()
        {
            Mapper.Register<UnitOfWork, UnitOfWorkViewModel>();
            Mapper.Compile();

            var unitOfWork = new UnitOfWork
            {
                Id = Guid.NewGuid(),
                State = States.InProgress
            };

            var unitOfWorkViewModel = unitOfWork.Map<UnitOfWork, UnitOfWorkViewModel>();
            Assert.AreEqual((int)unitOfWork.State, (int)unitOfWorkViewModel.State);
            Assert.AreEqual(unitOfWork.Id, unitOfWorkViewModel.Id);
        }

        [Test]
        public void ParallelPrecompileCollectionTest()
        {
            Mapper.Register<Composition, CompositionViewModel>()
                .Member(dest => dest.Booking, src => src.Booking);
            Mapper.Register<Booking, BookingViewModel>();
            Mapper.Compile();

            var actions = new List<Action>();

            for (var i = 0; i < 100; i++)
            {
                actions.Add(() => Mapper.PrecompileCollection<List<Booking>, IEnumerable<BookingViewModel>>());
            }

            Parallel.Invoke(actions.ToArray());
        }

        [Test]
        public void DefaultPrimitiveTypePropertyToStringTest()
        {
            Mapper.Register<TestDefaultDecimal, TestDefaultDecimalToStringViewModel>()
                .Member(dest => dest.TestString, src => src.TestDecimal);
            Mapper.RegisterCustom<decimal, string>(src => src.ToString("#0.00", CultureInfo.InvariantCulture));
            Mapper.Compile();
            var test = new TestDefaultDecimal() { TestDecimal = default(decimal) };
            test.TestDecimal = default(decimal);
            var result = Mapper.Map<TestDefaultDecimal, TestDefaultDecimalToStringViewModel>(test);
            //This is where the mapping fails
            Assert.AreEqual(result.TestString, "0.00");
        }

        [Test]
        public void MemberMappingsHasHigherPriorityThanCaseSensetiveTest()
        {
            Mapper.Register<Source, TargetViewModel>()
                .Member(t => t.Enabled, s => s.enabled == "Y");
            Mapper.Compile();

            var source = new Source { enabled = "N" };
            var result = Mapper.Map<Source, TargetViewModel>(source);
            Assert.AreEqual(result.Enabled, false);
        }

        [Test]
        public void FieldsTest()
        {
            Mapper.Register<Brand, BrandViewModel>();
            Mapper.Register<Table, TableViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var srcAndDest = Functional.FieldsTestMap();

            var bvm = Mapper.Map<Table, TableViewModel>(srcAndDest.Key);

            Assert.AreEqual(bvm, srcAndDest.Value);
        }

        [Test]
        public void CustomMemberAndFunctionFieldsTest()
        {
            Mapper.Register<Brand, BrandViewModel>();
            Mapper.Register<Table, TableViewModel>()
                .Member(t => t.Id, t => t.Id)
                .Function(t => t.Name, t => t.Name);
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var srcAndDest = Functional.FieldsTestMap();

            var bvm = Mapper.Map<Table, TableViewModel>(srcAndDest.Key);

            Assert.AreEqual(bvm, srcAndDest.Value);
        }

        [Test]
        public void MappingOrderLessTest()
        {
            Mapper.Register<Composition, CompositionViewModel>()
                .Member(dest => dest.Booking, src => src.Booking);
            Mapper.Register<Booking, BookingViewModel>();

            var srcAndDest = Functional.RecursiveCompilationAssociationTestMap();

            var bvm = Mapper.Map<Booking, BookingViewModel>(srcAndDest.Key);

            Assert.AreEqual(bvm, srcAndDest.Value);
        }

        [Test]
        public void RecursiveCompilationDirectCollectionTest()
        {
            Mapper.Register<Employee, EmployeeViewModel>();
            Mapper.Compile();

            var srcAndDest = Functional.RecursiveCompilationDirectCollectionTestMap();

            var bvm = Mapper.Map<Employee, EmployeeViewModel>(srcAndDest.Key);

            Assert.AreEqual(bvm, srcAndDest.Value);
        }

        [Test]
        public void RecursiveCompilationDirectAssociationTest()
        {
            Mapper.Register<Person, PersonViewModel>();
            Mapper.Compile();

            var srcAndDest = Functional.RecursiveCompilationDirectAssociationTestMap();
            var bvm = Mapper.Map<Person, PersonViewModel>(srcAndDest.Key);

            Assert.AreEqual(bvm, srcAndDest.Value);
        }

        [Test]
        public void RecursiveCompilationAssociationTest()
        {
            Mapper.Register<Booking, BookingViewModel>();
            Mapper.Register<Composition, CompositionViewModel>();
            Mapper.Compile();

            var srcAndDest = Functional.RecursiveCompilationAssociationTestMap();

            var bvm = Mapper.Map<Booking, BookingViewModel>(srcAndDest.Key);

            Assert.AreEqual(bvm, srcAndDest.Value);
        }

        [Test]
        public void RecursiveCompilationCollectionTest()
        {
            Mapper.Register<Engine, EngineViewModel>();
            Mapper.Register<Cylinder, CylinderViewModel>();
            Mapper.Compile();

            var srcAndDest = Functional.RecursiveCompilationCollectionTestMap();
            var engineViewModel = Mapper.Map<Engine, EngineViewModel>(srcAndDest.Key);
            Assert.AreEqual(engineViewModel, srcAndDest.Value);
        }

        [Test]
        public void DynamicMapCollectionPropertyTest()
        {
            Mapper.Register<Engine, EngineViewModel>();
            Mapper.Compile();

            var srcAndDest = Functional.RecursiveCompilationCollectionTestMap();
            var engineViewModel = Mapper.Map<Engine, EngineViewModel>(srcAndDest.Key);
            Assert.AreEqual(engineViewModel, srcAndDest.Value);
        }


        [Test]
        public void DirectDynamicMapTest()
        {
            var srcAndDest = Functional.RecursiveCompilationCollectionTestMap();
            var engineViewModel = Mapper.Map<Engine, EngineViewModel>(srcAndDest.Key);
            Assert.AreEqual(engineViewModel, srcAndDest.Value);
        }


        [Test]
        public void CompilelessMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();

            var test = Functional.AutoMemberMap();

            var testViewModel = Mapper.Map<TestModel, TestViewModel>(test.Key);

            Assert.AreEqual(testViewModel, test.Value);
        }

        [Test]
        public void DynamicMapTest()
        {
            var test = Functional.AutoMemberMap();
            var testViewModel = Mapper.Map<TestModel, TestViewModel>(test.Key);
            Assert.AreEqual(testViewModel, test.Value);
        }

        [Test]
        public void AutoMemberMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var test = Functional.AutoMemberMap();

            var testViewModel = Mapper.Map<TestModel, TestViewModel>(test.Key);

            Assert.AreEqual(testViewModel, test.Value);
        }

        [Test]
        public void AutoMemberMapDeepCopy()
        {
            Mapper.Register<TestModel, TestModel>();
            Mapper.Register<Size, Size>();
            Mapper.Register<Country, Country>();
            Mapper.Compile();

            var test = Functional.AutoMemberMap();

            var deepCopy = Mapper.Map<TestModel, TestModel>(test.Key);

            Assert.AreNotEqual(deepCopy.GetHashCode(), test.Key.GetHashCode());
            Assert.AreNotEqual(deepCopy.Country.GetHashCode(), test.Key.Country.GetHashCode());
            for (var i = 0; i < deepCopy.Sizes.Count; i++)
            {
                Assert.AreNotEqual(deepCopy.Sizes[i].GetHashCode(), test.Key.Sizes[i].GetHashCode());
            }
            Assert.AreEqual(deepCopy, test.Key);
        }

        [Test]
        public void ManualPrimitiveMemberMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Member(src => src.Name, dest => string.Format("Full - {0} - Size", dest.Alias))
                .Member(src => src.SortOrder, dest => dest.Id.GetHashCode())
                .Member(src => src.Nullable, dest => dest.Nullable)
                .Member(src => src.NotNullable, dest => dest.NotNullable)
                .Member(src => src.BoolValue, dest => dest.BoolValue);
            Mapper.Compile();

            var sizeResult = Functional.ManualPrimitiveMemberMap();

            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);

            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void HiddenInheritedMemberMap()
        {
            Mapper.Register<SpecialPerson, SpecialPersonViewModel>();

            var mapConfig = Mapper.Register<SpecialGift, SpecialGiftViewModel>();
            MapBaseMember(mapConfig);
            mapConfig.Member(src => src.Recipient, dst => dst.Recipient);

            Mapper.Compile();

            var srcDst = Functional.HiddenInheritedMemberMap();

            var result = Mapper.Map<SpecialGift, SpecialGiftViewModel>(srcDst.Key);

            Assert.AreEqual(result, srcDst.Value);
        }

        private void MapBaseMember<T, TN>(IMemberConfiguration<T, TN> mapConfig)
            where T : Gift
            where TN : GiftViewModel
        {
            mapConfig.Member(src => src.Recipient, dst => dst.Recipient);
        }

        [Test]
        public void ManualConstantMemberMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Value(src => src.SortOrder, 123)
                .Value(src => src.BoolValue, true);
            Mapper.Compile();

            var sizeResult = Functional.ManualPrimitiveMemberMap();

            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);

            Assert.AreEqual(result.SortOrder, 123);
            Assert.AreEqual(result.BoolValue, true);
        }

        [Test]
        public void ManualNestedNotNullMemberMap()
        {
            Mapper.Register<Trip, TripViewModel>()
                .Member(src => src.Name, dest => dest.Category.Name)
                .Ignore(x => x.Category);
            Mapper.Compile();

            var source = new Trip()
            {
                Category = new CategoryTrip()
                {
                    Name = "TestCat123"
                },
                Name = "abc"
            };

            var result = Mapper.Map<Trip, TripViewModel>(source);

            Assert.IsNull(result.Category);
            Assert.AreEqual(result.Name, "TestCat123");
        }

        [Test]
        public void ManualNestedNullMemberMap()
        {
            Mapper.Register<Trip, TripViewModel>()
                .Member(src => src.Name, dest => dest.Category.Name)
                .Ignore(x => x.Category);
            Mapper.Compile();

            var source = new Trip()
            {
                Name = "abc"
            };

            var result = Mapper.Map<Trip, TripViewModel>(source);

            Assert.IsNull(result.Category);
            Assert.IsNull(result.Name);
        }

        [Test]
        public void InstantiateMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Instantiate(src => new SizeViewModel(s => string.Format("{0} - Full name - {1}", src.Id, s)));
            Mapper.Compile();

            var sizeResult = Functional.InstantiateMap();

            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);

            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void InstantiateFuncMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .InstantiateFunc(src =>
                {
                    return new SizeViewModel(s => string.Format("{0} - Full name - {1}", src.Id, s));
                });
            Mapper.Compile();

            var sizeResult = Functional.InstantiateMap();

            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);

            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void IgnoreMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .Ignore(dest => dest.Name);
            Mapper.Compile();

            var sizeResult = Functional.IgnoreMap();
            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);
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
            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void BeforeMapDuplicateTest()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => Mapper.Register<Size, SizeViewModel>()
                .Before((src, dest) => dest.Name = src.Name)
                .Before((src, dest) => dest.Name = src.Name)
                .Ignore(dest => dest.Name));
            Assert.That(exception.Message,
                Is.EqualTo("BeforeMap already registered for ExpressMapper.Tests.Model.Models.Size"));

            var sizeResult = Functional.BeforeMap();
            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void AfterMap()
        {
            Mapper.Register<Size, SizeViewModel>()
                .After((src, dest) => dest.Name = "OVERRIDE BY AFTER MAP");
            Mapper.Compile();
            var sizeResult = Functional.AfterMap();
            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void AfterMapDuplicateTest()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => Mapper.Register<Size, SizeViewModel>()
                .After((src, dest) => dest.Name = "OVERRIDE BY AFTER MAP")
                .After((src, dest) => dest.Name = "Duplicate map"));
            Assert.That(exception.Message,
                Is.EqualTo("AfterMap already registered for ExpressMapper.Tests.Model.Models.Size"));
            var sizeResult = Functional.AfterMap();
            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }


        [Test]
        public void CustomMapWithSupportedCollectionMaps()
        {
            Mapper.RegisterCustom<Size, SizeViewModel, SizeMapper>();
            Mapper.Compile();
            var sizeResult = Functional.CustomMap();
            var result = Mapper.Map<Size, SizeViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void CustomMapWithSupportedNestedCollectionMaps()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.RegisterCustom<Size, SizeViewModel, SizeMapper>();
            Mapper.Compile();
            var sizeResult = Functional.CustomNestedCollectionMap();
            var result = sizeResult.Item1.Map<TestModel, TestViewModel>();
            Assert.AreEqual(result, sizeResult.Item3);
        }

        [Test]
        public void DeepCopyCustomMapWithSupportedNestedCollectionMaps()
        {
            Mapper.Register<TestModel, TestModel>();
            Mapper.Register<Country, Country>();
            Mapper.RegisterCustom<Size, Size, DeepCopySizeMapper>();
            Mapper.Compile();
            var sizeResult = Functional.CustomNestedCollectionMap();
            var deepCopy = Mapper.Map<TestModel, TestModel>(sizeResult.Item1);

            Assert.AreNotEqual(deepCopy.GetHashCode(), sizeResult.Item1.GetHashCode());
            Assert.AreNotEqual(deepCopy.Country.GetHashCode(), sizeResult.Item1.Country.GetHashCode());
            for (var i = 0; i < deepCopy.Sizes.Count; i++)
            {
                Assert.AreNotEqual(deepCopy.Sizes[i].GetHashCode(), sizeResult.Item1.Sizes[i].GetHashCode());
            }
            Assert.AreEqual(deepCopy, sizeResult.Item1);
        }

        [Test]
        public void CustomMapListWithSupportedNestedCollectionMaps()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.RegisterCustom<List<Size>, List<SizeViewModel>, SizeListMapper>();
            Mapper.Compile();
            var sizeResult = Functional.CustomNestedCollectionMap();
            var result = Mapper.Map<TestModel, TestViewModel>(sizeResult.Item1);
            Assert.AreEqual(result, sizeResult.Item3);
        }

        [Test]
        public void ExistingDestCustomMapWithSupportedNestedCollectionMaps()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.RegisterCustom<Size, SizeViewModel, SizeMapper>();
            Mapper.Compile();
            var sizeResult = Functional.CustomNestedCollectionMap();
            var result = Mapper.Map(sizeResult.Item1, sizeResult.Item2);
            Assert.AreEqual(result, sizeResult.Item3);
        }

        [Test]
        public void ExistingDestCustomMapListWithSupportedNestedCollectionMaps()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.RegisterCustom<List<Size>, List<SizeViewModel>, SizeListMapper>();
            Mapper.Compile();
            var sizeResult = Functional.CustomNestedCollectionMap();
            var result = Mapper.Map(sizeResult.Item1, sizeResult.Item2);
            Assert.AreEqual(result, sizeResult.Item3);
        }

        [Test]
        public void NullPropertyAndNullCollectionPropertyMaps()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Register<Size, SizeViewModel>();

            Mapper.Compile();
            var sizeResult = Functional.NullPropertyAndNullCollectionMap();
            var result = Mapper.Map<TestModel, TestViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void OnlyGetPropertyMaps()
        {
            Mapper.Register<Supplier, SupplierViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Compile();
            var supplierResult = Functional.GetPropertyMaps();
            var result = Mapper.Map<Supplier, SupplierViewModel>(supplierResult.Key);
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
            var result = Mapper.Map<Supplier, SupplierViewModel>(supplierResult.Key);
            Assert.AreEqual(result, supplierResult.Value);
        }

        [Test]
        public void CustomMap()
        {
            Mapper.RegisterCustom<GenderTypes, string>(g => g.ToString());
            Mapper.Compile();

            var result = Mapper.Map<GenderTypes, string>(GenderTypes.Men);
            Assert.AreEqual(result, GenderTypes.Men.ToString());
        }

        [Test]
        public void AutoMemberStructMap()
        {
            Mapper.Register<Item, ItemViewModel>();
            Mapper.Compile();
            var testData = Functional.AutoMemberStructMap();

            var result = Mapper.Map<Item, ItemViewModel>(testData.Key);
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

            var result = Mapper.Map<Item, ItemViewModel>(testData.Key);
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
                        default:
                            optionalGender = src.Gender;
                            break;
                    }
                    return optionalGender;
                });
            Mapper.Register<ProductOption, ProductOptionViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Feature, FeatureViewModel>();
            Mapper.Register<City, CityViewModel>()
                .Member(dest => dest.FeaturesList, src => src.Features);
            Mapper.Register<Supplier, SupplierViewModel>();
            Mapper.Register<Brand, BrandViewModel>();

            Mapper.Compile();
            var testData = Functional.ComplexMap();

            var result = Mapper.Map<FashionProduct, FashionProductViewModel>(testData.Key);
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
            var result = Mapper.Map<TestItem, TestItemViewModel>(typeCollTest.Key);
            Assert.AreEqual(result.Array.Length, typeCollTest.Key.List.Count);
        }

        [Test]
        public void ListNullToArray()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Array, src => src.List)
                .Ignore(dest => dest.Collection)
                .Ignore(dest => dest.Enumerable)
                .Ignore(dest => dest.List)
                .Ignore(dest => dest.Queryable);

            Mapper.Compile();

            var typeCollTest = new TestItem()
            {
                List = null
            };

            var result = Mapper.Map<TestItem, TestItemViewModel>(typeCollTest);

            Assert.IsNull(result.Array);
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
            var result = Mapper.Map<TestItem, TestItemViewModel>(typeCollTest.Key);
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
            var result = Mapper.Map<TestItem, TestItemViewModel>(typeCollTest.Key);
            Assert.AreEqual(result.Queryable.Count(), typeCollTest.Key.Enumerable.Count());
        }

        [Test]
        public void QueryableToArray()
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
            var result = Mapper.Map<TestItem, TestItemViewModel>(typeCollTest.Key);
            Assert.AreEqual(result.Array.Count(), typeCollTest.Key.Queryable.Count());
        }

        [Test]
        public void NonGenericSimpleMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var test = Functional.AutoMemberMap();

            var testViewModel = Mapper.Map(test.Key, typeof(TestModel), typeof(TestViewModel)) as TestViewModel;

            Assert.AreEqual(testViewModel, test.Value);
        }

        [Test]
        public void NonGenericSimpleWithDestinationMap()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var test = Functional.AutoMemberMap();

            var resultInstanceHash = test.Value.GetHashCode();
            var testViewModel =
                Mapper.Map(test.Key, test.Value, typeof(TestModel), typeof(TestViewModel)) as TestViewModel;

            Assert.AreEqual(testViewModel.GetHashCode(), resultInstanceHash);
            Assert.AreEqual(testViewModel, test.Value);
        }

        [Test]
        public void CustomMapNonGeneric()
        {
            Mapper.RegisterCustom<Size, SizeViewModel, SizeMapper>();
            Mapper.Compile();
            var sizeResult = Functional.CustomMap();
            var result = Mapper.Map(sizeResult.Key, typeof(Size), typeof(SizeViewModel));
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void AccessSourceNestedProperty()
        {
            Mapper.Register<TestModel, TestViewModel>()
                .Member(dest => dest.Name, src =>
                        $"Test - {src.Country.Name} - and date: {DateTime.Now} plus {src.Country.Code}");
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Register<Size, SizeViewModel>();

            Mapper.Compile();

            var sizeResult = Functional.AccessSourceNestedProperty();

            var result = Mapper.Map<TestModel, TestViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void AccessSourceManyNestedProperties()
        {
            Mapper.Register<Trip, TripViewModel>()
                .Member(dest => dest.Name, src =>
                        $"Type: {src.Category.Catalog.TripType.Name}, Catalog: {src.Category.Catalog.Name}, Category: {src.Category.Name}")
                .Ignore(dest => dest.Category);

            Mapper.Compile();

            var sizeResult = Functional.AccessSourceManyNestedProperties();

            var result = Mapper.Map<Trip, TripViewModel>(sizeResult.Key);
            Assert.AreEqual(result, sizeResult.Value);
        }

        [Test]
        public void ExistingDestinationSimple()
        {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Register<Size, SizeViewModel>();

            var sizeResult = Functional.ExistingDestinationSimpleMap();

            var testObjHash = sizeResult.Value.GetHashCode();
            var countryHash = sizeResult.Value.Country.GetHashCode();
            var sizesHash = sizeResult.Value.Sizes.GetHashCode();
            var sizeHashesList = new List<int>(sizeResult.Value.Sizes.Count);
            sizeHashesList.AddRange(sizeResult.Value.Sizes.Select(size => size.GetHashCode()));

            var result = sizeResult.Key.Map(sizeResult.Value);
            Assert.AreEqual(result, sizeResult.Value);
            Assert.AreEqual(result.GetHashCode(), testObjHash);
            Assert.AreEqual(result.Country.GetHashCode(), countryHash);
            Assert.AreEqual(result.Sizes.GetHashCode(), sizesHash);

            for (var i = 0; i < result.Sizes.Count; i++)
            {
                Assert.AreEqual(result.Sizes[i].GetHashCode(), sizeHashesList[i]);
            }
        }

        [Test]
        public void ExistingDestCollEquals()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Array, src => src.Queryable)
                .Ignore(dest => dest.Queryable);

            Mapper.Compile();
            var testResult = Functional.ExistingDestCollEquals();

            var testItemHash = testResult.Item2.GetHashCode();
            var arrayHash = testResult.Item2.Array.GetHashCode();
            var testArr = new List<int>(testResult.Item2.Array.Length);
            testArr.AddRange(testResult.Item2.Array.Select(tc => tc.GetHashCode()));


            var result = Mapper.Map(testResult.Item1, testResult.Item2);
            Assert.AreEqual(result, testResult.Item2);
            Assert.AreEqual(result.GetHashCode(), testItemHash);
            Assert.AreEqual(result.Array.GetHashCode(), arrayHash);

            for (var i = 0; i < result.Array.Length; i++)
            {
                Assert.AreEqual(result.Array[i].GetHashCode(), testArr[i]);
                Assert.AreEqual(result.Array[i], testResult.Item3.Array[i]);
            }
        }

        [Test]
        public void ExistingDestCollEqualsWithNullElement()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Array, src => src.Queryable)
                .Ignore(dest => dest.Queryable);

            Mapper.Compile();
            var testResult = Functional.ExistingDestCollEqualsWithNullElement();

            var testItemHash = testResult.Item2.GetHashCode();
            var arrayHash = testResult.Item2.Array.GetHashCode();
            var testArr = new List<int?>(testResult.Item2.Array.Length);
            testArr.AddRange(testResult.Item2.Array.Select(tc => tc == null ? (int?)null : tc.GetHashCode()));

            var result = Mapper.Map(testResult.Item1, testResult.Item2);
            Assert.AreEqual(result, testResult.Item2);
            Assert.AreEqual(result.GetHashCode(), testItemHash);
            Assert.AreEqual(result.Array.GetHashCode(), arrayHash);

            for (var i = 0; i < result.Array.Length; i++)
            {
                if (i == 3)
                {
                    Assert.AreEqual(null, result.Array[i]);
                    Assert.AreEqual(null, testArr[i]);
                }
                else
                {
                    Assert.AreEqual(result.Array[i].GetHashCode(), testArr[i]);
                    Assert.AreEqual(result.Array[i], testResult.Item3.Array[i]);
                }
            }
        }

        [Test]
        public void OtherCollectionTypesTest()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.ObservableCollection, src => src.Array)
                .Ignore(dest => dest.Array);

            Mapper.Compile();
            var testResult = Functional.OtherCollectionMapTest();

            var result = Mapper.Map<TestItem, TestItemViewModel>(testResult.Key);
            Assert.AreEqual(result.ObservableCollection.Count, testResult.Key.Array.Length);

            for (var i = 0; i < result.ObservableCollection.Count; i++)
            {
                Assert.AreEqual(result.ObservableCollection[i], testResult.Value.ObservableCollection[i]);
            }
        }

        [Test]
        public void ExistingSrcCollGreater()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Collection, src => src.Array)
                .Ignore(dest => dest.Array)
                .Ignore(dest => dest.List)
                .Ignore(dest => dest.Enumerable)
                .Ignore(dest => dest.ObservableCollection)
                .Ignore(dest => dest.Queryable);

            Mapper.Compile();
            var testResult = Functional.ExistingDestSrcCollGreater();

            var testItemHash = testResult.Item2.GetHashCode();
            var collectionHash = testResult.Item2.Collection.GetHashCode();
            var testColl = new List<int>(testResult.Item2.Collection.Count);
            testColl.AddRange(testResult.Item2.Collection.Select(tc => tc.GetHashCode()));

            var result = Mapper.Map(testResult.Item1, testResult.Item2);
            Assert.AreEqual(result, testResult.Item2);
            Assert.AreEqual(result.GetHashCode(), testItemHash);
            Assert.AreEqual(result.Collection.GetHashCode(), collectionHash);

            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(result.Collection.ElementAt(i).GetHashCode(), testColl[i]);
            }

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(result.Collection.ElementAt(i), testResult.Item3.Collection.ElementAt(i));
            }
        }

        [Test]
        public void ExistingDestDestCollGreater()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.List, src => src.Collection)
                .Ignore(dest => dest.Collection);

            Mapper.Compile();
            var testResult = Functional.ExistingDestCollGreater();

            var testItemHash = testResult.Item2.GetHashCode();
            var listHash = testResult.Item2.List.GetHashCode();
            var testList = new List<int>(testResult.Item2.List.Count);
            testList.AddRange(testResult.Item2.List.Select(tc => tc.GetHashCode()));


            var result = Mapper.Map(testResult.Item1, testResult.Item2);
            Assert.AreEqual(result, testResult.Item2);
            Assert.AreEqual(result.GetHashCode(), testItemHash);
            Assert.AreEqual(result.List.GetHashCode(), listHash);

            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(result.List.ElementAt(i).GetHashCode(), testList[i + 4]);
            }

            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(result.List[i], testResult.Item3.List[i]);
            }
        }

        [Test]
        public void NewDestinationTest()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.List, src => src.Collection)
                .Ignore(dest => dest.Collection);

            Mapper.Compile();
            var testResult = Functional.ExistingDestCollGreater();

            var testList = new List<int>(testResult.Item2.List.Count);
            testList.AddRange(testResult.Item2.List.Select(tc => tc.GetHashCode()));


            var result = Mapper.Map(testResult.Item1, new TestItemViewModel());

            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(result.List[i], testResult.Item3.List[i]);
            }
        }

        [Test]
        public void ExistingDestCollNotEqual()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Register<TestItem, TestItemViewModel>()
                .Member(dest => dest.Array, src => src.Collection)
                .Ignore(dest => dest.Collection);

            Mapper.Compile();
            var testResult = Functional.ExistingDestCollNotEqual();

            var testItemHash = testResult.Item2.GetHashCode();
            var listHash = testResult.Item2.Array.GetHashCode();
            var testList = new List<int>(testResult.Item2.Array.Length);
            testList.AddRange(testResult.Item2.Array.Select(tc => tc.GetHashCode()));

            var result = Mapper.Map(testResult.Item1, testResult.Item2);

            Assert.AreEqual(result, testResult.Item2);
            Assert.AreEqual(result.GetHashCode(), testItemHash);
            Assert.AreNotEqual(result.Array.GetHashCode(), listHash);

            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(result.Array[i].GetHashCode(), testList[i]);
                Assert.AreEqual(result.Array[i], testResult.Item3.Array[i]);
            }
        }

        [Test]
        public void ExistingDestinationComplex()
        {
            Mapper.Register<ItemModel, ItemModelViewModel>();
            Mapper.Register<SubItem, SubItemViewModel>();
            Mapper.Register<Unit, UnitViewModel>();
            Mapper.Register<SubUnit, SubUnitViewModel>();
            Mapper.Compile();

            var sizeResult = Functional.ExistingDestinationComplex();

            var itemModelHash = sizeResult.Value.GetHashCode();
            var itemModelSubItemsHash = sizeResult.Value.SubItems.GetHashCode();

            var subItemsHashes = new List<int>(sizeResult.Value.SubItems.Length);
            var subItemUnitsCollHashes = new Dictionary<int, int>();
            var subItemUnitsHashes = new Dictionary<int, List<int>>();
            var subItemUnitSubUnitCollHashes = new Dictionary<int, List<int>>();
            var subItemUnitSubUnitsHashes = new Dictionary<int, Dictionary<int, List<int>>>();

            foreach (var subItem in sizeResult.Value.SubItems)
            {
                var sbHash = subItem.GetHashCode();
                subItemsHashes.Add(sbHash);
                subItemUnitsCollHashes.Add(sbHash, subItem.Units.GetHashCode());
                subItemUnitsHashes.Add(sbHash, new List<int>());
                subItemUnitSubUnitCollHashes.Add(sbHash, new List<int>());
                subItemUnitSubUnitsHashes.Add(sbHash, new Dictionary<int, List<int>>());

                foreach (var unit in subItem.Units.Skip(1))
                {
                    subItemUnitsHashes[sbHash].Add(unit.GetHashCode());
                    subItemUnitSubUnitCollHashes[sbHash].Add(unit.SubUnits.GetHashCode());
                    subItemUnitSubUnitsHashes[sbHash][unit.GetHashCode()] = new List<int>();

                    foreach (var subUnit in unit.SubUnits)
                    {
                        subItemUnitSubUnitsHashes[sbHash][unit.GetHashCode()].Add(subUnit.GetHashCode());
                    }
                }
            }

            var result = Mapper.Map(sizeResult.Key, sizeResult.Value);

            Assert.AreEqual(result, sizeResult.Value);
            Assert.AreEqual(result.GetHashCode(), itemModelHash);
            Assert.AreEqual(result.SubItems.GetHashCode(), itemModelSubItemsHash);
            for (var i = 0; i < result.SubItems.Length; i++)
            {
                Assert.AreEqual(result.SubItems[i].GetHashCode(), subItemsHashes[i]);
                Assert.AreEqual(result.SubItems[i].Units.GetHashCode(),
                    subItemUnitsCollHashes[result.SubItems[i].GetHashCode()]);

                for (var j = 0; j < 4; j++)
                {
                    Assert.AreEqual(result.SubItems[i].Units[j].GetHashCode(),
                        subItemUnitsHashes[result.SubItems[i].GetHashCode()][j]);
                    Assert.AreEqual(result.SubItems[i].Units[j].SubUnits.GetHashCode(),
                        subItemUnitSubUnitCollHashes[result.SubItems[i].GetHashCode()][j]);
                    for (var k = 0; k < 3; k++)
                    {
                        Assert.AreEqual(result.SubItems[i].Units[j].SubUnits[k].GetHashCode(),
                            subItemUnitSubUnitsHashes[result.SubItems[i].GetHashCode()][
                                result.SubItems[i].Units[j].GetHashCode()][k]);
                    }
                }
            }
        }

        [Test]
        public void ExistingDestinationMedium()
        {
            Mapper.Register<TripType, TripTypeViewModel>();
            Mapper.Register<TripCatalog, TripCatalogViewModel>();
            Mapper.Register<CategoryTrip, CategoryTripViewModel>();
            Mapper.Register<Trip, TripViewModel>();

            Mapper.Compile();
            var tripResult = Functional.ExistingDestinationMediumMap();

            var tripHash = tripResult.Value.GetHashCode();
            var tripCatHash = tripResult.Value.Category.GetHashCode();
            var tripCatCtlHash = tripResult.Value.Category.Catalog.GetHashCode();
            var tripCatCtlTypeHash = tripResult.Value.Category.Catalog.TripType.GetHashCode();


            var result = Mapper.Map<Trip, TripViewModel>(tripResult.Key, tripResult.Value);
            Assert.AreEqual(result, tripResult.Value);
            Assert.AreEqual(result.GetHashCode(), tripHash);
            Assert.AreEqual(result.Category.GetHashCode(), tripCatHash);
            Assert.AreEqual(result.Category.Catalog.GetHashCode(), tripCatCtlHash);
            Assert.AreEqual(result.Category.Catalog.TripType.GetHashCode(), tripCatCtlTypeHash);
        }

        [Test]
        public void EnumMap()
        {
            Mapper.Register<TestModel, TestViewModel>()
                .Ignore(x => x.Country)
                .Ignore(x => x.Sizes)
                .Member(x => x.GenderIndex, x => x.NullableGender);
            Mapper.Compile();

            var test = new TestModel()
            {
                Gender = GenderTypes.Men.ToString(),
                NullableGender = GenderTypes.Women
            };

            var testViewModel = Mapper.Map<TestModel, TestViewModel>(test);

            Assert.AreEqual(GenderTypes.Men, testViewModel.Gender);
            Assert.AreEqual(GenderTypes.Women.ToString(), testViewModel.NullableGender);
            Assert.AreEqual((int)GenderTypes.Women, testViewModel.GenderIndex);
        }

        [Test]
        public void ConvertibleMap()
        {
            Mapper.Register<TestModel, TestViewModel>()
                .Ignore(x => x.Country)
                .Ignore(x => x.Sizes)
                .Member(x => x.GenderIndex, x => x.BoolValue)
                .Member(x => x.NotNullable, x => x.Height);
            Mapper.Compile();

            var test = new TestModel()
            {
                BoolValue = true,
                Height = 123
            };

            var testViewModel = Mapper.Map<TestModel, TestViewModel>(test);

            Assert.AreEqual(1, testViewModel.GenderIndex);
            Assert.AreEqual(123, testViewModel.NotNullable);
        }

        [Test]
        public void MemberCaseInSensitivityDefaultMapTest()
        {
            Mapper.Register<TypoCase, TypoCaseViewModel>();
            Mapper.Compile();

            var typoCase = new TypoCase
            {
                id = Guid.NewGuid(),
                NaME = "Test name!",
                TestId = 5
            };

            var typoCaseViewModel = Mapper.Map<TypoCase, TypoCaseViewModel>(typoCase);
            Assert.AreEqual(typoCase.id, typoCaseViewModel.Id);
            Assert.AreEqual(typoCase.NaME, typoCaseViewModel.Name);
            Assert.AreEqual(typoCase.TestId, typoCaseViewModel.TestId);
        }

        [Test]
        public void MemberCaseSensitivityGlobalMapTest()
        {
            Mapper.MemberCaseSensitiveMap(true);
            Mapper.Register<TypoCase, TypoCaseViewModel>();
            Mapper.Compile();

            var typoCase = new TypoCase
            {
                id = Guid.NewGuid(),
                NaME = "Test name!",
                TestId = 5
            };

            var typoCaseViewModel = Mapper.Map<TypoCase, TypoCaseViewModel>(typoCase);

            Assert.AreEqual(typoCaseViewModel.Id, Guid.Empty);
            Assert.AreEqual(typoCaseViewModel.Name, null);
            Assert.AreEqual(typoCase.TestId, typoCaseViewModel.TestId);
        }

        [Test]
        public void MemberCaseSensitivityLocalMapTest()
        {
            Mapper.Register<TypoCase, TypoCaseViewModel>()
                .CaseSensitive(true);
            Mapper.Compile();

            var typoCase = new TypoCase
            {
                id = Guid.NewGuid(),
                NaME = "Test name!",
                TestId = 5
            };

            var typoCaseViewModel = Mapper.Map<TypoCase, TypoCaseViewModel>(typoCase);

            Assert.AreEqual(typoCaseViewModel.Id, Guid.Empty);
            Assert.AreEqual(typoCaseViewModel.Name, null);
            Assert.AreEqual(typoCase.TestId, typoCaseViewModel.TestId);
        }

        [Test]
        public void MemberCaseInSensitivityGlobalOverrideMapTest()
        {
            Mapper.MemberCaseSensitiveMap(true);
            Mapper.Register<TypoCase, TypoCaseViewModel>()
                .CaseSensitive(false);
            Mapper.Compile();

            var typoCase = new TypoCase
            {
                id = Guid.NewGuid(),
                NaME = "Test name!",
                TestId = 5
            };

            var typoCaseViewModel = Mapper.Map<TypoCase, TypoCaseViewModel>(typoCase);
            Assert.AreEqual(typoCase.id, typoCaseViewModel.Id);
            Assert.AreEqual(typoCase.NaME, typoCaseViewModel.Name);
            Assert.AreEqual(typoCase.TestId, typoCaseViewModel.TestId);
        }

        [Test]
        public void InheritanceFuncMap()
        {
            Mapper.Register<Contact, ContactViewModel>();
            Mapper.Register<Mail, MailViewModel>()
                .Member(x => x.Contact, x => ResolveContact(x.Contact))
                .Member(x => x.StandardContactVM, x => x.StandardContact);
            Mapper.Compile();

            var contactId = Guid.NewGuid();

            var test = new Mail()
            {
                From = "from",
                Contact = new Organization()
                {
                    Id = contactId,
                    Name = "org"
                }
            };

            var testViewModel = Mapper.Map<Mail, MailViewModel>(test);

            Assert.AreEqual("from", testViewModel.From);
            Assert.AreEqual(contactId, testViewModel.Contact.Id);
            Assert.IsInstanceOf(typeof(OrganizationViewModel), testViewModel.Contact);
        }

        private static ContactViewModel ResolveContact(Contact contact)
        {
            ContactViewModel destination = null;

            if (contact.IsOrganization)
            {
                destination = new OrganizationViewModel();

                Mapper.Map(contact, destination);
            }
            else if (contact.IsPerson)
            {
                destination = new PersonViewModel();

                Mapper.Map(contact, destination);
            }
            else
            {
                destination = new ContactViewModel();

                Mapper.Map(contact, destination);
            }

            return destination;
        }

        [Test]
        public void ClearErrorDuringDynamicMappingTest()
        {
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Name = "Ticket",
                Venue = new Venue
                {
                    Id = Guid.NewGuid(),
                    Name = "Venue"
                }
            };


            Assert.Throws<ExpressmapperException>(() =>
            {
                Mapper.Map<Ticket, TicketViewModel>(ticket);
            });
        }

        [Test]
        public void ClearErrorDuringCompileTimeTest()
        {
            Mapper.Register<Ticket, TicketViewModel>();

            Assert.Throws<ExpressmapperException>(Mapper.Compile);
        }

        [Test]
        public void MapExistsTest()
        {
            Mapper.Register<Father, FlattenFatherSonGrandsonDto>();

            Assert.True(Mapper.MapExists(typeof(Father), typeof(FlattenFatherSonGrandsonDto)));
            Assert.False(Mapper.MapExists(typeof(FlattenFatherSonGrandsonDto), typeof(Father)));
        }

        //[Test]
        //public void DoNotUpdateUnchangedPropertyValuesTest()
        //{
        //    var srcBrand = new Brand
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = "brand"
        //    };

        //    var existingBrandMock = new Mock<Brand>().SetupAllProperties();
        //    existingBrandMock.Object.Name = "brand";

        //    var destBrand = Mapper.Map(srcBrand, existingBrandMock.Object);
        //    existingBrandMock.VerifySet(x => x.Name = It.IsAny<string>(), Times.Once());

        //    Assert.AreEqual(destBrand.Id, srcBrand.Id);
        //    Assert.AreEqual(destBrand.Name, srcBrand.Name);
        //}


        [Test]
        public void InheritanceIncludeTest()
        {
            Mapper.Register<BaseControl, BaseControlViewModel>()
                .Member(dst => dst.id_ctrl, src => src.Id)
                .Member(dst => dst.name_ctrl, src => src.Name)
                .Include<TextBox, TextBoxViewModel>();
            Mapper.Compile();

            var textBox = new TextBox
            {
                Id = Guid.NewGuid(),
                Name = "Just a text box",
                Description = "Just a text box - very simple description",
                Text = "Hello World!"
            };

            var baseControlViewModel = Mapper.Map<BaseControl, BaseControlViewModel>(textBox);
            Assert.AreEqual(baseControlViewModel.id_ctrl, textBox.Id);
            Assert.AreEqual(baseControlViewModel.name_ctrl, textBox.Name);
            Assert.IsTrue(baseControlViewModel is TextBoxViewModel);
        }

        [Test]
        public void NestedInheritanceIncludeTest()
        {
            Mapper.Register<BaseControl, BaseControlViewModel>()
                .Member(dst => dst.id_ctrl, src => src.Id)
                .Member(dst => dst.name_ctrl, src => src.Name)
                .Include<TextBox, TextBoxViewModel>();
            Mapper.Register<UserInterface, UserInterfaceViewModel>()
                .Member(dest => dest.ControlViewModel, src => src.Control);
            Mapper.Compile();

            var textBox = new TextBox
            {
                Id = Guid.NewGuid(),
                Name = "Just a text box",
                Description = "Just a text box - very simple description",
                Text = "Hello World!"
            };

            var userInterface = new UserInterface
            {
                Control = textBox
            };

            var uiViewModel = Mapper.Map<UserInterface, UserInterfaceViewModel>(userInterface);
            Assert.NotNull(uiViewModel.ControlViewModel);
            Assert.True(uiViewModel.ControlViewModel is TextBoxViewModel);
            Assert.AreEqual(uiViewModel.ControlViewModel.Description, textBox.Description);
            Assert.AreEqual(uiViewModel.ControlViewModel.id_ctrl, textBox.Id);
            Assert.AreEqual(uiViewModel.ControlViewModel.name_ctrl, textBox.Name);
            Assert.AreEqual(((TextBoxViewModel)uiViewModel.ControlViewModel).Text, textBox.Text);
        }

        [Test]
        public void MapNullSourceReturnNullDest()
        {
            Mapper.Register<object, object>();
            Mapper.Compile();

            Assert.IsNull(Mapper.Map<object, object>(null));
            Assert.IsNull(Mapper.Map<object, object>(null, (object)null));
        }

        #region Duplicate property names in the class hierarchy

        [Test]
        public void MapDuplicatePropertyNamesInHierarchyTest()
        {
            Mapper.Register<A, AN>();
            Mapper.Register<AN, A>();

            Mapper.Register<T, TNT>();
            Mapper.Register<TNT, T>();
            Mapper.Compile();


            var tnt = new TNT
            {
                Foo = new AN
                {
                    Id = 4
                }
            };

            var t = new T
            {
                Foo = new A
                {
                    Id = 5
                }
            };

            var tntResult = t.Map<T, TNT>();
            var tResult = tnt.Map<TNT, T>();

            Assert.AreEqual(tntResult.Foo.Id, 5);
            Assert.AreEqual(tResult.Foo.Id, 4);
        }

        class A
        {
            public int Id { get; set; }
        }

        class AN : A
        {
            public new int Id { get; set; }
        }

        class T
        {
            public A Foo { get; set; }
        }

        class TN : T
        {
            public new AN Foo { get; set; }
        }

        class TNT : TN
        {
            public new AN Foo { get; set; }
        }

        #endregion
    }
}
