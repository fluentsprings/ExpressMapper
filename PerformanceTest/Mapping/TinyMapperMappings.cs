using Nelibur.ObjectMapper;
using PerformanceTest.Models;
using PerformanceTest.Tests;
using PerformanceTest.ViewModels;

namespace PerformanceTest.Mapping
{
    public class TinyMapperMappings
    {
        public static void Init()
        {
            TinyMapper.Bind<ProductVariant, ProductVariantViewModel>();

            TinyMapper.Bind<News, NewsViewModel>();

            TinyMapper.Bind<Item, ItemViewModel>();
        }
    }
}
