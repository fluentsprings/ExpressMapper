using ExpressMapper;
using PerformanceTest.Enums;
using PerformanceTest.Models;
using PerformanceTest.Tests;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Mapping
{
    public static class ExpressMapperMapping
    {
        public static void Init()
        {
            Mapper.Register<ProductVariant, ProductVariantViewModel>();
            Mapper.Register<Product, ProductViewModel>()
                .Member(dest => dest.DefaultSharedOption, src => src.DefaultOption);
            Mapper.Register<Test, TestViewModel>()
                .Before((src, dest) => dest.Age = src.Age)
                .After((src, dest) => dest.Weight = src.Weight * 2)
                .Ignore(dest => dest.Age)
                .Member(dest => dest.Type, src => (Types)src.Type)
                .Member(dest => dest.Name, src => string.Format("{0} - {1} - {2}", src.Name, src.Weight, src.Age))
                .Function(dest => dest.SpareTheProduct, src => src.SpareProduct)
                .Instantiate(src => new TestViewModel(string.Format("{0} - {1}", src.Name, src.Id)));

            Mapper.Register<News, NewsViewModel>();

            Mapper.Register<Role, RoleViewModel>();
            Mapper.Register<User, UserViewModel>()
                .Member(dest => dest.BelongTo, src => src.Role);

            Mapper.Register<Article, ArticleViewModel>();
            Mapper.Register<Author, AuthorViewModel>()
                .Function(dest => dest.OwnedArticles, src => src.Articles);

            Mapper.Register<Item, ItemViewModel>();
            Mapper.Compile();
        }

        public static void InitAdvanced()
        {
            Mapper.Register<ProductVariant, ProductVariantViewModel>();
            Mapper.Register<Product, ProductViewModel>()
                .Member(dest => dest.DefaultSharedOption, src => src.DefaultOption);
            Mapper.Register<Test, TestViewModel>()
                .Member(dest => dest.Age, src => src.Age)
                .Member(dest => dest.Weight, src => src.Weight * 2)
                .Member(dest => dest.Type, src => (Types)src.Type)
                .Member(dest => dest.Name, src => string.Format("{0} - {1} - {2}", src.Name, src.Weight, src.Age))
                .Member(dest => dest.SpareTheProduct, src => src.SpareProduct)
                .Member(dest => dest.Description, src => string.Format("{0} - {1}", src.Name, src.Id));
            Mapper.Compile();
        }
    }
}
