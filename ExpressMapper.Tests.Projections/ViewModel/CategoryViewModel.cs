using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class CategoryViewModel : IEquatable<CategoryViewModel>
    {
        public CategoryViewModel()
        {
            Products = new List<ProductViewModel>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<ProductViewModel> Products { get; set; }

        public bool Equals(CategoryViewModel other)
        {
            var productsEquals = ((Products != null && other.Products != null) && Products.Count() == other.Products.Count()) || ((other.Products == null && Products == null));
            if (productsEquals && Products != null && other.Products != null && Products.Where((t, i) => !t.Equals(other.Products.ElementAt(i))).Any())
            {
                productsEquals = false;
            }
            return Id == other.Id && Name == other.Name && productsEquals;
        }
    }
}
