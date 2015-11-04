using System;
using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Tests.Projections.Entities;
using ExpressMapper.Tests.Projections.ViewModel;
using NUnit.Framework;

namespace ExpressMapper.Tests.Projections.Tests
{
    [TestFixture]
    public class CollectionPropertyTest : BaseTest
    {
        private List<CategoryViewModel> _result; 
        #region Initialize data

        private void InitializeData()
        {
            var size = new Size
            {
                Id = Guid.NewGuid(),
                Name = "Medium",
                Code = "M"
            };

            var productVariant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                Name = "Orange",
                Color = "Orange",
                Size = size,
                SizeId = size.Id
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Blue Jeans",
                Dimensions = "23x56x21",
                Variant = productVariant,
                VariantId = productVariant.Id
            };


            var productVariant1 = new ProductVariant
            {
                Id = Guid.NewGuid(),
                Name = "Orange",
                Color = "Orange"
            };

            var product1 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Blue Jeans",
                Dimensions = "23x56x21",
                Variant = productVariant1,
                VariantId = productVariant1.Id
            };

            var product2 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Precious",
                Dimensions = "23x56x21"
            };

            Context.Set<Size>().Add(size);
            Context.Set<ProductVariant>().Add(productVariant);
            Context.Set<Product>().Add(product);

            Context.Set<ProductVariant>().Add(productVariant1);
            Context.Set<Product>().Add(product1);

            Context.Set<Product>().Add(product2);


            var category = new Category
            {
              Id  = Guid.NewGuid(),
              Name = "Halloween",
              Products = new List<Product>
              {
                  product, product2, product1
              }
            };

            var category1 = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Drive",
                Products = new List<Product>
              {
                  product2, product1
              }
            };

            var category2 = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Empty products"
            };

            Context.Set<Category>().Add(category);
            Context.Set<Category>().Add(category1);
            Context.Set<Category>().Add(category2);

            Context.SaveChanges();
        }

        #endregion

        #region Register mappings

        private static void RegisterMappings()
        {
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
            _result = Context.Set<Category>().Project<Category, CategoryViewModel>().ToList();
        }

        [Test]
        public void Test()
        {
            Assert.AreEqual(_result.Count, 3);
        }
    }
}
