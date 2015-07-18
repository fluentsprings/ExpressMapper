using System.Collections.Generic;
using System.Linq;
using Benchmarks.Enums;
using Benchmarks.Models;
using Benchmarks.ViewModels;
using Omu.ValueInjecter;

namespace Benchmarks.Mapping
{
    public class ValueInjectorMappings
    {
        public static void Init()
        {
            Mapper.Reset();

            Mapper.AddMap<Product, ProductViewModel>(src =>
            {
                var productViewModel = new ProductViewModel();
                productViewModel.InjectFrom(src);
                productViewModel.DefaultSharedOption =
                    Mapper.Map<ProductVariant, ProductVariantViewModel>(src.DefaultOption);
                productViewModel.Options = new List<ProductVariantViewModel>();
                foreach (var pv in src.Options)
                {
                    productViewModel.Options.Add(Mapper.Map<ProductVariant, ProductVariantViewModel>(pv));
                }
                return productViewModel;
            });

            Mapper.AddMap<Test, TestViewModel>(src =>
            {
                var testViewModel = new TestViewModel(string.Format("{0} - {1}", src.Name, src.Id));
                testViewModel.InjectFrom(src);
                
                testViewModel.Name = string.Format("{0} - {1} - {2}", src.Name, src.Weight, src.Age);

                testViewModel.Product = Mapper.Map<Product, ProductViewModel>(src.Product);
                testViewModel.SpareTheProduct = Mapper.Map<Product, ProductViewModel>(src.SpareProduct);
                testViewModel.Type = (Types) src.Type;
                testViewModel.Weight = src.Weight*2;
                testViewModel.Products = new List<ProductViewModel>();
                foreach (var product in src.Products)
                {
                    testViewModel.Products.Add(Mapper.Map<Product, ProductViewModel>(product));
                }
                return testViewModel;
            });

            Mapper.AddMap<User, UserViewModel>(src =>
            {
                var userViewModel = new UserViewModel();
                userViewModel.InjectFrom(src);
                userViewModel.BelongTo = Mapper.Map<Role, RoleViewModel>(src.Role);
                return userViewModel;
            });

            Mapper.AddMap<Author, AuthorViewModel>(src =>
            {
                var articles = new ArticleViewModel[src.Articles.Count()];
                var authorViewModel = new AuthorViewModel();
                authorViewModel.InjectFrom(src);
                
                for (var i = 0; i < articles.Length; i++)
                {
                    articles[i] = Mapper.Map<Article, ArticleViewModel>(src.Articles.ElementAt(i));
                }
                authorViewModel.OwnedArticles = articles;
                return authorViewModel;
            });
        }
    }
}
