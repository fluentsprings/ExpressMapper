using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class CatalogueViewModel : IEquatable<CatalogueViewModel>
    {
        public CatalogueViewModel()
        {
            Categories = new List<CategoryViewModel>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<CategoryViewModel> Categories { get; set; }

        public bool Equals(CatalogueViewModel other)
        {
            var categoriesEquals = ((Categories != null && other.Categories != null) && Categories.Count() == other.Categories.Count()) || ((other.Categories == null && Categories == null));
            if (categoriesEquals && Categories != null && other.Categories != null && Categories.Where((t, i) => !t.Equals(other.Categories.ElementAt(i))).Any())
            {
                categoriesEquals = false;
            }
            return Id == other.Id && Name == other.Name && categoriesEquals;
        }
    }
}
