using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class ProductConfiguration : EntityTypeConfiguration<Product>
    {
        public ProductConfiguration()
        {
            ToTable("Product");
            HasKey(p => p.Id);
            HasOptional(p => p.Variant).WithMany(v => v.Products).HasForeignKey(p => p.VariantId);
        }
    }
}
