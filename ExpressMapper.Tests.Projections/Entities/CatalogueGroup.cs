using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class CatalogueGroup
    {
        public CatalogueGroup()
        {
            Catalogues = new List<Catalogue>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual List<Catalogue> Catalogues { get; set; }
    }
}
