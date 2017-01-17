using System;
using ExpressMapper.Tests.Model.Generator;
using ExpressMapper.Tests.Model.Models;
using ExpressMapper.Tests.Model.ViewModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

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

            var result = Mapper.Map<List<TestModel>, List<TestViewModel>>(testData.Key);

            Assert.AreEqual(result.Count, testData.Value.Count);

            for (var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void AutoMemberMap_ToNonGenericListInherited() {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var testData = Functional.CollectionAutoMemberMap();

            var result = Mapper.Map<List<TestModel>, NonGenericCollectionInhertedFromList>( testData.Key );

            Assert.AreEqual( result.Count, testData.Value.Count );

            for ( var i = 0; i < result.Count; i++ ) {
                Assert.AreEqual( result[i], testData.Value[i] );
            }
        }

        [Test]
        public void AutoMemberMap_ToLinkedList() {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var testData = Functional.CollectionAutoMemberMap();

            var result = Mapper.Map<List<TestModel>, LinkedList<TestViewModel>>( testData.Key );

            Assert.AreEqual( result.Count, testData.Value.Count );

            var resultList = result.ToList();

            for ( var i = 0; i < resultList.Count; i++ ) {
                Assert.AreEqual( resultList[i], testData.Value[i] );
            }
        }

        [Test]
        public void AutoMemberMap_ToNonGenericEnumerableInherited() {
            Mapper.Register<TestModel, TestViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Country, CountryViewModel>();
            Mapper.Compile();

            var testData = Functional.CollectionAutoMemberMap();

            var result = Mapper.Map<List<TestModel>, NonGenericCollectionImplementingIEnumerable>( testData.Key );

            var resultList = result.ToList();

            Assert.AreEqual( resultList.Count, testData.Value.Count );

            for ( var i = 0; i < resultList.Count; i++ ) {
                Assert.AreEqual( resultList[i], testData.Value[i] );
            }
        }

        [Test]
        public void DirectDynamicCollectionMapTest()
        {
            var testData = Functional.CollectionAutoMemberMap();
            var result = Mapper.Map<List<TestModel>, List<TestViewModel>>(testData.Key);
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

            var result = Mapper.Map<IEnumerable<TestCollection>, List<TestCollectionViewModel>>(testData.Key);

            Assert.AreEqual(result.Count(), testData.Value.Count);

            for (var i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void EnumerationToQueryableTypeMap()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Compile();

            var testData = Functional.EnumerableToListTypeMap();

            var result = Mapper.Map<IEnumerable<TestCollection>, IQueryable<TestCollectionViewModel>>(testData.Key);

            Assert.AreEqual(result.Count(), testData.Value.Count);

            for (var i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(result.ElementAt(i), testData.Value[i]);
            }
        }

        [Test]
        public void EnumerationToArrayTypeMap()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Compile();

            var testData = Functional.EnumerableToArrayTypeMap();

            var result = Mapper.Map<IEnumerable<TestCollection>, List<TestCollectionViewModel>>(testData.Key);

            Assert.AreEqual(result.Count(), testData.Value.Length);

            for (var i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(result[i], testData.Value[i]);
            }
        }

        [Test]
        public void ArrayToListTypeMap()
        {
            Mapper.Register<TestCollection, TestCollectionViewModel>();
            Mapper.Compile();

            var testData = Functional.EnumerableToArrayTypeMap();

            var result = Mapper.Map<TestCollection[], IList<TestCollectionViewModel>>(testData.Key.ToArray());

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

            var result = Mapper.Map<List<TestModel>, List<TestViewModel>>(testData.Key);

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

        [Test]
        public void AutoMemberMapCollectionDeepCopy()
        {
            Mapper.Register<TestModel, TestModel>();
            Mapper.Register<Size, Size>();
            Mapper.Register<Country, Country>();
            Mapper.Compile();

            var test = Functional.AutoMemberMapCollection();

            var deepCopies = Mapper.Map<List<TestModel>, List<TestModel>>(test.Key);

            Assert.AreEqual(deepCopies.Count, test.Key.Count);
            for(var i = 0; i < deepCopies.Count; i++)
            {
                Assert.AreNotEqual(deepCopies[i].GetHashCode(), test.Key[i].GetHashCode());
                Assert.AreNotEqual(deepCopies[i].Country.GetHashCode(), test.Key[i].Country.GetHashCode());
                for (var j = 0; j < deepCopies[i].Sizes.Count; j++)
                {
                    Assert.AreNotEqual(deepCopies[i].Sizes[j].GetHashCode(), test.Key[i].Sizes[j].GetHashCode());
                }
                Assert.AreEqual(deepCopies[i], test.Key[i]);
            }
        }

        [Test]
        public void CollectionWithDestinationMap()
        {
            Mapper.Register<DbObjectA, ObjectA>();
            Mapper.Register<DbObjectB, ObjectB>().
                Member(x => x.Childs, x => x.DbObjectA);
            Mapper.Compile();

            var objectB1 = new DbObjectB()
            {
                Code = "ObjectB1",
                DbObjectA = new DbObjectA[] { new DbObjectA() { Code = "ObjectA1" } }
            };

            var objectB2 = new DbObjectB()
            {
                Code = "ObjectB2",
                DbObjectA = new DbObjectA[] { new DbObjectA() { Code = "ObjectA2" } }
            };

            DbObjectB[] objectBArr = new DbObjectB[] { objectB1, objectB2 };

            List<ObjectB> result = new List<ObjectB>();

            Mapper.Map<DbObjectB[], List<ObjectB>>(objectBArr, result);

            Assert.AreEqual(2, result.Count);

            Assert.IsNotNull(result[0]);
            Assert.IsNotNull(result[0].Childs);
            Assert.AreEqual(1, result[0].Childs.Count);
            Assert.AreEqual("ObjectB1", result[0].Code);
            Assert.AreEqual("ObjectA1", result[0].Childs[0].Code);

            Assert.IsNotNull(result[1]);
            Assert.IsNotNull(result[1].Childs);
            Assert.AreEqual(1, result[1].Childs.Count);
            Assert.AreEqual("ObjectB2", result[1].Code);
            Assert.AreEqual("ObjectA2", result[1].Childs[0].Code);
        }

        [Test]
        public void InheritanceIncludeTest()
        {
            Mapper.Register<BaseControl, BaseControlViewModel>()
                .Member(dst => dst.id_ctrl, src => src.Id)
                .Member(dst => dst.name_ctrl, src => src.Name)
                .Include<TextBox, TextBoxViewModel>()
                .Include<ComboBox, ComboBoxViewModel>();

            Mapper.Register<ComboBox, ComboBoxViewModel>()
                .Member(dest => dest.AmountOfElements, src => src.NumberOfElements);

            Mapper.Compile();

            var textBox = new TextBox
            {
                Id = Guid.NewGuid(),
                Name = "Just a text box",
                Description = "Just a text box - very simple description",
                Text = "Hello World!"
            };

            var comboBox = new ComboBox
            {
                Id = Guid.NewGuid(),
                Name = "Just a combo box",
                Description = "Just a combo box - very simple description",
                GeneralName = "Super Combo mombo",
                NumberOfElements = 103
            };

            var controls = new List<BaseControl>
            {
                textBox, comboBox
            };

            var controlVms = Mapper.Map<List<BaseControl>, IEnumerable<BaseControlViewModel>>(controls);
            Assert.NotNull(controlVms);
            Assert.True(controlVms.Any());
            Assert.True(controlVms.Any(c => c is TextBoxViewModel));
            Assert.True(controlVms.Any(c => c is ComboBoxViewModel));
        }

        [Test]
        public void NestedInheritanceIncludeTest()
        {
            Mapper.Register<BaseControl, BaseControlViewModel>()
                .Member(dst => dst.id_ctrl, src => src.Id)
                .Member(dst => dst.name_ctrl, src => src.Name)
                .Include<TextBox, TextBoxViewModel>()
                .Include<ComboBox, ComboBoxViewModel>();

            Mapper.Register<ComboBox, ComboBoxViewModel>()
                .Member(dest => dest.AmountOfElements, src => src.NumberOfElements);

            Mapper.Register<Gui, GuiViewModel>()
                .Member(dest => dest.ControlViewModels, src => src.Controls);
            Mapper.Compile();

            var textBox = new TextBox
            {
                Id = Guid.NewGuid(),
                Name = "Just a text box",
                Description = "Just a text box - very simple description",
                Text = "Hello World!"
            };

            var comboBox = new ComboBox
            {
                Id = Guid.NewGuid(),
                Name = "Just a combo box",
                Description = "Just a combo box - very simple description",
                GeneralName = "Super Combo mombo",
                NumberOfElements = 103
            };

            var gui = new Gui()
            {
                Controls = new List<BaseControl>
                {
                    textBox, comboBox
                }
            };

            var guiViewModel = Mapper.Map<Gui, GuiViewModel>(gui);
            Assert.NotNull(guiViewModel.ControlViewModels);
            Assert.True(guiViewModel.ControlViewModels.Any());
            Assert.True(guiViewModel.ControlViewModels.Any(c => c is TextBoxViewModel));
            Assert.True(guiViewModel.ControlViewModels.Any(c => c is ComboBoxViewModel));
        }

        public class ObjectA
        {
            public string Code { get; set; }
        }

        public class ObjectB
        {
            public string Code { get; set; }

            public List<ObjectA> Childs { get; set; }
        }

        public class DbObjectA
        {
            public string Code { get; set; }
        }

        public class DbObjectB
        {
            public string Code { get; set; }

            public DbObjectA[] DbObjectA { get; set; }
        }
    }
}
