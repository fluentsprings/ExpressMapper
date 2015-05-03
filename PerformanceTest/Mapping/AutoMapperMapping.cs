using AutoMapper;
using PerformanceTest.Enums;
using PerformanceTest.Models;
using PerformanceTest.Tests;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Mapping
{
    public static class AutoMapperMapping
    {
        public static void Init()
        {
            Mapper.Reset();
            Mapper.CreateMap<ProductVariant, ProductVariantViewModel>();
            Mapper.CreateMap<Product, ProductViewModel>()
                .ForMember(dest => dest.DefaultSharedOption, src => src.MapFrom(m => m.DefaultOption));
            Mapper.CreateMap<Test, TestViewModel>()
                .BeforeMap((src, dest) => dest.Age = src.Age)
                .AfterMap((src, dest) => dest.Weight = src.Weight * 2)
                .ForMember(dest => dest.Age, src => src.Ignore())
                .ForMember(dest => dest.Type, src => src.MapFrom(m => (Types)m.Type))
                .ForMember(dest => dest.Name, src => src.MapFrom(m => string.Format("{0} - {1} - {2}", m.Name, m.Weight, m.Age)))
                .ForMember(dest => dest.SpareTheProduct, src => src.ResolveUsing(m => m.SpareProduct))
                .ConstructUsing((src => new TestViewModel(string.Format("{0} - {1}", src.Name, src.Id))));

            Mapper.CreateMap<News, NewsViewModel>();

            Mapper.CreateMap<Role, RoleViewModel>();
            Mapper.CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.BelongTo, src => src.MapFrom(m => m.Role));

            Mapper.CreateMap<Article, ArticleViewModel>();
            Mapper.CreateMap<Author, AuthorViewModel>()
                .ForMember(dest => dest.OwnedArticles, src => src.ResolveUsing(m => m.Articles));

            Mapper.CreateMap<Item, ItemViewModel>();
        }
    }
}
