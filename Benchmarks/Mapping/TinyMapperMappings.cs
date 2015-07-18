using Benchmarks.Models;
using Benchmarks.ViewModels;
using Nelibur.ObjectMapper;

namespace Benchmarks.Mapping
{
    public class TinyMapperMappings
    {
        public static void Init()
        {
            TinyMapper.Bind<ProductVariant, ProductVariantViewModel>();
            TinyMapper.Bind<Product, ProductViewModel>(config =>
                {
                    config.Bind(src => src.DefaultOption, dest => dest.DefaultSharedOption);
                });
            TinyMapper.Bind<Test, TestViewModel>(config =>
                {
                    //config.Before((src, dest) => dest.Age = src.Age)
                    //config.After((src, dest) => dest.Weight = src.Weight * 2)
                    //config.Instantiate(src => new TestViewModel(string.Format("{0} - {1}", src.Name, src.Id)));
                    config.Ignore(dest => dest.Age);
                    config.Bind(src => src.Type, dest => dest.Type);
                    //config.Bind(src => string.Format("{0} - {1} - {2}", src.Name, src.Weight, src.Age), dest => dest.Name);
                    config.Bind(src => src.SpareProduct, dest => dest.SpareTheProduct);
                });

            TinyMapper.Bind<News, NewsViewModel>();

            TinyMapper.Bind<Role, RoleViewModel>();
            TinyMapper.Bind<User, UserViewModel>(config =>
                {
                    config.Bind(src => src.Role, dest => dest.BelongTo);
                });

            TinyMapper.Bind<Article, ArticleViewModel>();
            TinyMapper.Bind<Author, AuthorViewModel>(config =>
                {
                    //config.Bind(src => src.Articles, dest => dest.OwnedArticles);
                });

            TinyMapper.Bind<Item, ItemViewModel>();
        }
    }
}
