using System;

namespace ExpressMapper.Tests.Model.ViewModels.Structs
{
    public class FeatureViewModel : IEquatable<FeatureViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Rank { get; set; }
        public bool Equals(FeatureViewModel other)
        {
            return Id == other.Id && Name == other.Name && Description == other.Description && Rank == other.Rank;
        }
    }
}
