using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class SizeConfiguration : EntityTypeConfiguration<Size>
    {
        public SizeConfiguration()
        {
            ToTable("Size");
            HasKey(s => s.Id);
        }
    }
}
