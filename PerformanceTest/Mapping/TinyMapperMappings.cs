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

            TinyMapper.Bind<News, NewsViewModel>();

            TinyMapper.Bind<Item, ItemViewModel>();
        }
    }
}
