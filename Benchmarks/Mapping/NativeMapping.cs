using System.Linq;
using Benchmarks.Enums;
using Benchmarks.Models;
using Benchmarks.ViewModels;

namespace Benchmarks.Mapping
{
    public static class NativeMapping
    {
        public static ItemViewModel Map(Item src)
        {
            return new ItemViewModel()
            {
                Id = src.Id,
                Name = src.Name,
                Height = src.Height,
                Length = src.Length,
                Weight = src.Weight,
                Width = src.Width
            };
        }

        public static TestViewModel Map(Test src)
        {
            if (src == null)
            {
                return default(TestViewModel);
            }
            var dst = new TestViewModel(string.Format("{0} - {1}", src.Name, src.Id));
            dst.Id = src.Id;
            dst.Created = src.Created;
            dst.Age = src.Age;
            dst.Name = string.Format("{0} - {1} - {2}", src.Name, src.Weight, src.Age);
            dst.Type = (Types)src.Type;
            dst.Weight = src.Weight;

            dst.Product = Map(src.Product);
            dst.SpareTheProduct = Map(src.SpareProduct);
            dst.Products = src.Products.Select(Map).ToList();
            return dst;
        }

        public static ProductViewModel Map(Product src)
        {
            if (src == null)
            {
                return default(ProductViewModel);
            }
            var dst = new ProductViewModel();
            dst.Id = src.Id;
            dst.Description = src.Description;
            dst.ProductName = src.ProductName;
            dst.Weight = src.Weight;
            dst.DefaultSharedOption = Map(src.DefaultOption);
            dst.Options = src.Options.Select(Map).ToList();
            return dst;
        }

        public static NewsViewModel Map(News src)
        {
            return new NewsViewModel()
            {
                Id = src.Id,
                IsXml = src.IsXml,
                Provider = src.Provider,
                StartDate = src.StartDate,
                Url = src.Url
            };
        }

        public static ProductVariantViewModel Map(ProductVariant src)
        {
            if (src == null)
            {
                return default(ProductVariantViewModel);
            }
            var dst = new ProductVariantViewModel();
            dst.Id = src.Id;
            dst.Color = src.Color;
            dst.Size = src.Size;
            return dst;
        }

        public static UserViewModel Map(User src)
        {
            if (src == null)
            {
                return default(UserViewModel);
            }
            var dst = new UserViewModel();
            dst.Id = src.Id;
            dst.Active = src.Active;
            dst.CreatedOn = src.CreatedOn;
            dst.Deleted = src.Deleted;
            dst.UserName = src.UserName;
            dst.Email = src.Email;
            dst.Address = src.Address;
            dst.Age = src.Age;
            dst.BelongTo = Map(src.Role);

            return dst;
        }

        public static RoleViewModel Map(Role src)
        {
            if (src == null)
            {
                return default(RoleViewModel);
            }
            var dst = new RoleViewModel();
            dst.Id = src.Id;
            dst.Active = src.Active;
            dst.CreatedOn = src.CreatedOn;
            dst.Deleted = src.Deleted;
            dst.Name = src.Name;
            return dst;
        }

        public static ArticleViewModel Map(Article src)
        {
            if (src == null)
            {
                return default(ArticleViewModel);
            }
            var dst = new ArticleViewModel();
            dst.Id = src.Id;
            dst.CreatedOn = src.CreatedOn;
            dst.Text = src.Text;
            dst.Title = src.Title;

            return dst;
        }

        public static AuthorViewModel Map(Author src)
        {
            if (src == null)
            {
                return default(AuthorViewModel);
            }
            var dst = new AuthorViewModel();
            dst.Id = src.Id;
            dst.Age = src.Age;
            dst.FirstName = src.FirstName;
            dst.LastName = src.LastName;
            dst.OwnedArticles = src.Articles.Select(Map).ToList();

            return dst;
        }
    }
}
