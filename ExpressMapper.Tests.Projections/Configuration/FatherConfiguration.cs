using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class FatherConfiguration : EntityTypeConfiguration<Father>
    {
        public FatherConfiguration()
        {
            ToTable("Father");
            HasKey(t => t.Id);
            HasRequired(t => t.Son);
        }
    }
}