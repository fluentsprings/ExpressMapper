using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class ProductVariantConfiguration : EntityTypeConfiguration<ProductVariant>
    {
        public ProductVariantConfiguration()
        {
            ToTable("ProductVariant");
            HasKey(p => p.Id);
            HasOptional(p => p.Size).WithMany(s => s.ProductVariants).HasForeignKey(p => p.SizeId);
        }
    }
}
