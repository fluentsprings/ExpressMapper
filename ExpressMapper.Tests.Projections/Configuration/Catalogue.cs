using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class CatalogueConfiguration : EntityTypeConfiguration<Catalogue>
    {
        public CatalogueConfiguration()
        {
            ToTable("Catalogue");
            HasKey(t => t.Id);
            HasMany(t => t.Categories).WithMany(t => t.Catalogues);
        }
    }
}
