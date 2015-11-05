using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class FullCatalogueViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<CatalogueGroupViewModel> CatalogueGroups { get; set; }
    }
}
