using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class BookingViewModel : IEquatable<BookingViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CompositionViewModel Composition { get; set; }


        public bool Equals(BookingViewModel other)
        {
            return Id == other.Id && Name == other.Name && ((Composition == null && other.Composition == null) || Composition.Equals(other.Composition));
        }
    }
}
