using System;
using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Extensions;
using ExpressMapper.Tests.Projections.Entities;
using ExpressMapper.Tests.Projections.ViewModel;
using NUnit.Framework;

namespace ExpressMapper.Tests.Projections.Tests
{
    [TestFixture]
    public class CollectionPropertyTest : BaseTest
    {
        private List<CatalogueGroupViewModel> _result;
        private List<CatalogueGroupViewModel> _planResult = new List<CatalogueGroupViewModel>();

        #region Initialize data

        private void InitializeData()
        {
            var sizeId = Guid.NewGuid();
            var sizeName = "Medium";
            var sizeCode = "M";

            var size = new Size
            {
                Id = sizeId,
                Name = sizeName,
                Code = sizeCode
            };

            var sizeVm = new SizeViewModel
            {
                Id = sizeId,
                Name = sizeName,
                Code = sizeCode
            };

            var productVarId = Guid.NewGuid();
            var productVarName = "Orange";
            var productVarColor = "Orange";
            var productVariant = new ProductVariant
            {
                Id = productVarId,
                Name = productVarName,
                Color = productVarColor,
                Size = size,
                SizeId = size.Id
            };

            var productVariantVm = new ProductVariantViewModel
            {
                Id = productVarId,
                Name = productVarName,
                Color = productVarColor,
                Size = sizeVm
            };

            var productId = Guid.NewGuid();
            var prodName = "Blue Jeans";
            var prodDimensions = "23x56x21";
            var product = new Product
            {
                Id = productId,
                Name = prodName,
                Dimensions = prodDimensions,
                Variant = productVariant,
                VariantId = productVariant.Id
            };

            var productViewModel = new ProductViewModel
            {
                Id = productId,
                Name = prodName,
                Dimensions = prodDimensions,
                Variant = productVariantVm
            };


            var prodVarId1 = Guid.NewGuid();
            var prodVarName1 = "Yellow";
            var prodVarColor1 = "Yellow";
            var productVariant1 = new ProductVariant
            {
                Id = prodVarId1,
                Name = prodVarName1,
                Color = prodVarColor1
            };

            var productVariantVm1 = new ProductVariantViewModel
            {
                Id = prodVarId1,
                Name = prodVarName1,
                Color = prodVarColor1
            };

            var prodId1 = Guid.NewGuid();
            var prodName1 = "Blue Jeans";
            var prodDimensions1 = "53x51x99";
            var product1 = new Product
            {
                Id = prodId1,
                Name = prodName1,
                Dimensions = prodDimensions1,
                Variant = productVariant1,
                VariantId = productVariant1.Id
            };

            var productVm1 = new ProductViewModel
            {
                Id = prodId1,
                Name = prodName1,
                Dimensions = prodDimensions1,
                Variant = productVariantVm1
            };

            var prodId2 = Guid.NewGuid();
            var prodName2 = "Precious";
            var prodDimensions2 = "13x36x61";
            var product2 = new Product
            {
                Id = prodId2,
                Name = prodName2,
                Dimensions = prodDimensions2
            };

            var prodVm2 = new ProductViewModel
            {
                Id = prodId2,
                Name = prodName2,
                Dimensions = prodDimensions2
            };

            Context.Set<Size>().Add(size);
            Context.Set<ProductVariant>().Add(productVariant);
            Context.Set<Product>().Add(product);

            Context.Set<ProductVariant>().Add(productVariant1);
            Context.Set<Product>().Add(product1);

            Context.Set<Product>().Add(product2);


            var catId = Guid.NewGuid();
            var catName = "Halloween";
            var category = new Category
            {
                Id = catId,
                Name = catName,
                Products = new List<Product>
              {
                  product, product2, product1
              }
            };

            var cat = new CategoryViewModel
            {
                Id = catId,
                Name = catName,
                Products = new List<ProductViewModel>
                {
                    productViewModel, productVm1, prodVm2
                }
            };

            var catId1 = Guid.NewGuid();
            var catName1 = "Test Drive";
            var category1 = new Category
            {
                Id = catId1,
                Name = catName1,
                Products = new List<Product>
              {
                  product2, product1
              }
            };

            var cat1 = new CategoryViewModel
            {
                Id = catId1,
                Name = catName1,
                Products = new List<ProductViewModel>
                {
                    prodVm2, productVm1
                }
            };

            var catId2 = Guid.NewGuid();
            var catName2 = "Empty products";
            var category2 = new Category
            {
                Id = catId2,
                Name = catName2
            };

            var cat2 = new CategoryViewModel
            {
                Id = catId2,
                Name = catName2
            };

            Context.Set<Category>().Add(category);
            Context.Set<Category>().Add(category1);
            Context.Set<Category>().Add(category2);

            var catalId = Guid.NewGuid();
            var catalName = "Catalogue # 1";
            var catalogue = new Catalogue
            {
                Id = catalId,
                Name = catalName
            };
            catalogue.Categories.AddRange(new[] { category, category1 });

            var catalVm = new CatalogueViewModel
            {
                Id = catalId,
                Name = catalName,
                Categories = new[]
                {
                    cat, cat1
                }
            };


            var catalId1 = Guid.NewGuid();
            var catalName1 = "Catalogue # 2";
            var catalogue1 = new Catalogue
            {
                Id = catalId1,
                Name = catalName1
            };
            catalogue1.Categories.AddRange(new[] { category, category1, category2 });

            var catalVm1 = new CatalogueViewModel
            {
                Id = catalId1,
                Name = catalName1,
                Categories = new []
                {
                    cat, cat1, cat2
                }
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

            var catalGrVm = new CatalogueGroupViewModel
            {
                Id = catalGrId,
                Name = catalGrName,
                Catalogues = new List<CatalogueViewModel>
                {
                   catalVm
                }
            };

            var catalGrId1 = Guid.NewGuid();
            var catalGrId2 = "CatalogueGroup #2";
            var catalogueGroup1 = new CatalogueGroup
            {
                Id = catalGrId1,
                Name = catalGrId2
            };
            catalogueGroup1.Catalogues.AddRange(new[] { catalogue, catalogue1 });

            var catalGrVm1 = new CatalogueGroupViewModel
            {
                Id = catalGrId1,
                Name = catalGrId2,
                Catalogues = new List<CatalogueViewModel>
                {
                    catalVm, catalVm1
                }
            };

            _planResult = new List<CatalogueGroupViewModel>
            {
                catalGrVm, catalGrVm1
            };

            Context.Set<CatalogueGroup>().Add(catalogueGroup);
            Context.Set<CatalogueGroup>().Add(catalogueGroup1);

            Context.SaveChanges();
        }

        #endregion

        #region Register mappings

        private static void RegisterMappings()
        {
            Mapper.Register<Catalogue, CatalogueViewModel>();
            Mapper.Register<CatalogueGroup, CatalogueGroupViewModel>();
            Mapper.Register<Product, ProductViewModel>();
            Mapper.Register<ProductVariant, ProductVariantViewModel>();
            Mapper.Register<Size, SizeViewModel>();
            Mapper.Register<Category, CategoryViewModel>();
        }

        #endregion

        protected override void Setup()
        {
            InitializeData();
            RegisterMappings();
        }

        protected override void Execute()
        {
            _result = Context.Set<CatalogueGroup>().Project<CatalogueGroup, CatalogueGroupViewModel>().ToList();
        }

        [Test]
        public void Test()
        {
            _result = SortCollections(_result);
            _planResult = SortCollections(_planResult);

            Assert.AreEqual(_result.Count, 2);
            for (var i = 0; i < _result.Count; i++)
            {
                Assert.AreEqual(_result[i], _planResult[i]);
            }
        }

        private List<CatalogueGroupViewModel> SortCollections(List<CatalogueGroupViewModel> list)
        {
            list.ForEach(r =>
            {
                r.Catalogues.ToList().ForEach(v =>
                {
                    if (v.Categories != null)
                    {
                        v.Categories.ToList().ForEach(e =>
                        {
                            if (e.Products != null)
                            {
                                e.Products = e.Products.OrderBy(p => p.Id).ToList();
                            }
                        }
                            );
                        v.Categories = v.Categories.OrderBy(f => f.Id).ToArray();
                    }
                });
                r.Catalogues = r.Catalogues.OrderBy(s => s.Id).ToList();
            });
            return list.OrderBy(r => r.Id).ToList();
        }
    }
}
