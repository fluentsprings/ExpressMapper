using System.Data.Entity;
using ExpressMapper.Tests.Projections.Configuration;

namespace ExpressMapper.Tests.Projections.Context
{
    public class ExpressContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new SizeConfiguration());
            modelBuilder.Configurations.Add(new ProductVariantConfiguration());
            modelBuilder.Configurations.Add(new ProductConfiguration());
            modelBuilder.Configurations.Add(new CategoryConfiguration());
            modelBuilder.Configurations.Add(new CatalogueConfiguration());
            modelBuilder.Configurations.Add(new CatalogueGroupConfiguration());
            //flattening tests entities
            modelBuilder.Configurations.Add(new FatherConfiguration());
            modelBuilder.Configurations.Add(new SonConfiguration());
            modelBuilder.Configurations.Add(new GrandsonConfiguration());
            modelBuilder.Configurations.Add(new FatherSonsConfiguration());
        }
    }
}
