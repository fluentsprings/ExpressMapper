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
    public class ComplexAssociationTest : BaseTest
    {
        private List<ProductViewModel> _result;
        private List<ProductViewModel> _planResult;

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

            Context.SaveChanges();

            _planResult = new List<ProductViewModel>
            {
                productViewModel, productVm1, prodVm2
            };
        }

        #endregion

        #region Register mappings

        private static void RegisterMappings()
        {
            Mapper.Register<Product, ProductViewModel>();
            Mapper.Register<ProductVariant, ProductVariantViewModel>();
            Mapper.Register<Size, SizeViewModel>();
        }

        #endregion

        protected override void Setup()
        {
            InitializeData();
            RegisterMappings();
        }

        protected override void Execute()
        {
            _result = Context.Set<Product>().Project<Product, ProductViewModel>().ToList();
        }

        [Test]
        public void Test()
        {
            _result = _result.OrderBy(p => p.Id).ToList();
            _planResult = _planResult.OrderBy(p => p.Id).ToList();
            Assert.AreEqual(_result.Count(), 3);
            for (var i = 0; i < _result.Count; i++)
            {
                Assert.AreEqual(_result[i], _planResult[i]);
            }
        }
    }
}