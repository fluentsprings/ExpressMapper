using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TripViewModel : IEquatable<TripViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CategoryTripViewModel Category { get; set; }
        public bool Equals(TripViewModel other)
        {
            return Id == other.Id && Name == other.Name && (Category == null || Category.Equals(other.Category));
        }
    }
}
