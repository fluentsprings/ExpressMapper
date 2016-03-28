using System;
using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Extensions;
using ExpressMapper.Tests.Projections.Entities;
using ExpressMapper.Tests.Projections.ViewModel;
using NUnit.Framework;

namespace ExpressMapper.Tests.Projections.Tests
{
    public class RecursionExpressionHandlingTest : BaseTest
    {
        private List<FullCatalogueGroupViewModel> _result;
        private List<FullCatalogueGroupViewModel> _planResult;

        #region Initialize data

        private void InitializeData()
        {
            var catalId = Guid.NewGuid();
            var catalName = "Catalogue # 1";
            var catalogue = new Catalogue
            {
                Id = catalId,
                Name = catalName
            };

            var cvm = new FullCatalogueViewModel
            {
                Id = catalId,
                Name = catalName
            };

            var catalId1 = Guid.NewGuid();
            var catalName1 = "Catalogue # 2";
            var catalogue1 = new Catalogue
            {
                Id = catalId1,
                Name = catalName1
            };

            var cvm1 = new FullCatalogueViewModel
            {
                Id = catalId1,
                Name = catalName1
            };

            Context.Set<Catalogue>().Add(catalogue);
            Context.Set<Catalogue>().Add(catalogue1);

            var catalGrId = Guid.NewGuid();
            var catalGrName = "CatalogueGroup #1";
            var catalogueGroup = new CatalogueGroup
            {
                Id = catalGrId,
                Name = catalGrName
            };
            catalogueGroup.Catalogues.Add(catalogue);

            var cvtgr = new FullCatalogueGroupViewModel
            {
                Id = catalGrId,
                Name = catalGrName,
                Catalogues = new List<FullCatalogueViewModel> { cvm }
            };

            var catalGrId1 = Guid.NewGuid();
            var catalGrName1 = "CatalogueGroup #2";
            var catalogueGroup1 = new CatalogueGroup
            {
                Id = catalGrId1,
                Name = catalGrName1
            };
            catalogueGroup1.Catalogues.AddRange(new[] { catalogue, catalogue1 });

            var cvtgr1 = new FullCatalogueGroupViewModel
            {
                Id = catalGrId1,
                Name = catalGrName1,
                Catalogues = new List<FullCatalogueViewModel> { cvm, cvm1 }
            };

            Context.Set<CatalogueGroup>().Add(catalogueGroup);
            Context.Set<CatalogueGroup>().Add(catalogueGroup1);

            Context.SaveChanges();
            _planResult = new List<FullCatalogueGroupViewModel>
            {
                cvtgr, cvtgr1
            };
        }

        #endregion

        #region Register mappings

        private static void RegisterMappings()
        {
            Mapper.Register<Catalogue, FullCatalogueViewModel>();
            Mapper.Register<CatalogueGroup, FullCatalogueGroupViewModel>();
        }

        #endregion

        protected override void Setup()
        {
            InitializeData();
            RegisterMappings();
        }

        protected override void Execute()
        {
            _result = Context.Set<CatalogueGroup>().Project<CatalogueGroup, FullCatalogueGroupViewModel>().ToList();
        }

        [Test]
        public void Test()
        {
            _result = SortCollections(_result);
            _planResult = SortCollections(_planResult);

            Assert.AreEqual(_result.Count, 2);
            for (var i = 0; i < _result.Count; i++)
            {
                Assert.AreEqual(_result[i].Id, _planResult[i].Id);
                Assert.AreEqual(_result[i].Name, _planResult[i].Name);
                for (var j = 0; j < _result[i].Catalogues.Count(); j++)
                {
                    Assert.AreEqual(_result[i].Catalogues.ElementAt(j).Id, _planResult[i].Catalogues.ElementAt(j).Id);
                    Assert.AreEqual(_result[i].Catalogues.ElementAt(j).Name, _planResult[i].Catalogues.ElementAt(j).Name);
                }
            }
        }

        private List<FullCatalogueGroupViewModel> SortCollections(List<FullCatalogueGroupViewModel> list)
        {
            list.ForEach(r =>
            {
                r.Catalogues = r.Catalogues.OrderBy(s => s.Id);
            });
            return list.OrderBy(r => r.Id).ToList();
        }
    }
}
