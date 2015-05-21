using System.Collections.Generic;
using Mapster;
using PerformanceTest.Enums;
using PerformanceTest.Models;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Mapping
{
    public class MapsterMapperMappings
    {
        public static void Init()
        {
            TypeAdapterConfig<Product, ProductViewModel>
                .NewConfig()
                .Map(dest => dest.DefaultSharedOption, src => TypeAdapter.Adapt<ProductVariant, ProductVariantViewModel>(src.DefaultOption));

            TypeAdapterConfig<Test, TestViewModel>
                .NewConfig()
                .Map(dest => dest.Age, src => src.Age)
                .Map(dest => dest.Weight, src => src.Weight * 2)
                .Map(dest => dest.Type, src => (Types)src.Type)
                .Map(dest => dest.Name, src => string.Format("{0} - {1} - {2}", src.Name, src.Weight, src.Age))
                .Map(dest => dest.Name, src => string.Format("{0} - {1} - {2}", src.Name, src.Weight, src.Age))
                .Map(dest => dest.SpareTheProduct, src => TypeAdapter.Adapt<Product, ProductViewModel>(src.SpareProduct))
                .Map(dest => dest.Description, src => string.Format("{0} - {1}", src.Name, src.Id));

            TypeAdapterConfig<User, UserViewModel>
                .NewConfig()
                .Map(dest => dest.BelongTo, src => TypeAdapter.Adapt<Role, RoleViewModel>(src.Role));


            TypeAdapterConfig<Author, AuthorViewModel>
                .NewConfig()
                .Map(dest => dest.OwnedArticles, src => TypeAdapter.Adapt<IEnumerable<Article>, ArticleViewModel[]>(src.Articles));
        }
    }
}
