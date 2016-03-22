using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class SonConfiguration : EntityTypeConfiguration<Son>
    {
        public SonConfiguration()
        {
            ToTable("Son");
            HasKey(t => t.Id);
            HasOptional(t => t.Grandson);
        }
    }
}