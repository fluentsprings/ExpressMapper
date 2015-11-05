using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Projections.Entities
{
    public class Catalogue
    {
        public Catalogue()
        {
            Categories = new List<Category>();
            CatalogueGroups = new List<CatalogueGroup>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public virtual List<Category> Categories { get; set; }
        public virtual List<CatalogueGroup> CatalogueGroups { get; set; }
    }
}
