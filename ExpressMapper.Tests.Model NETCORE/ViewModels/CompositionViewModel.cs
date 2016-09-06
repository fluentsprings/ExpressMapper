using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class CompositionViewModel : IEquatable<CompositionViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public BookingViewModel Booking { get; set; }


        public bool Equals(CompositionViewModel other)
        {
            return Id == other.Id && Name == other.Name && ((Booking == null && other.Booking == null) || Booking.Equals(other.Booking));
        }
    }
}
