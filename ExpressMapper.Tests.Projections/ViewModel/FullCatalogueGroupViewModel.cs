using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class FullCatalogueGroupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<FullCatalogueViewModel> Catalogues { get; set; }
    }
}
