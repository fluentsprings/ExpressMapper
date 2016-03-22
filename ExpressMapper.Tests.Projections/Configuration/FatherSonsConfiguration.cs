using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class FatherSonsConfiguration : EntityTypeConfiguration<FatherSons>
    {
        public FatherSonsConfiguration()
        {
            ToTable("FatherSons");
            HasKey(t => t.Id);
            HasMany(t => t.Sons);
        }
    }
}