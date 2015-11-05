using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class CatalogueGroupConfiguration : EntityTypeConfiguration<CatalogueGroup>
    {
        public CatalogueGroupConfiguration()
        {
            ToTable("CatalogueGroup");
            HasKey(t => t.Id);
            HasMany(t => t.Catalogues).WithMany(r => r.CatalogueGroups);
        }
    }
}
