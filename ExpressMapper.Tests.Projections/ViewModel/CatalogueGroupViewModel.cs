using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class CatalogueGroupViewModel : IEquatable<CatalogueGroupViewModel>
    {
        public CatalogueGroupViewModel()
        {
            Catalogues = new List<CatalogueViewModel>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<CatalogueViewModel> Catalogues { get; set; }

        public bool Equals(CatalogueGroupViewModel other)
        {
            var cataloguesEquals = ((Catalogues != null && other.Catalogues != null) && Catalogues.Count() == other.Catalogues.Count()) || ((other.Catalogues == null && Catalogues == null));
            if (cataloguesEquals && Catalogues != null && other.Catalogues != null && Catalogues.Where((t, i) => !t.Equals(other.Catalogues.ElementAt(i))).Any())
            {
                cataloguesEquals = false;
            }
            return Id == other.Id && Name == other.Name && cataloguesEquals;
        }
    }
}
