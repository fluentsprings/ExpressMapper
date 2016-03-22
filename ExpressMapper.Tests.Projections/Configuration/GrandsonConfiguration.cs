using System.Data.Entity.ModelConfiguration;
using ExpressMapper.Tests.Projections.Entities;

namespace ExpressMapper.Tests.Projections.Configuration
{
    public class GrandsonConfiguration : EntityTypeConfiguration<Grandson>
    {
        public GrandsonConfiguration()
        {
            ToTable("Grandson");
            HasKey(t => t.Id);
        }
    }
}