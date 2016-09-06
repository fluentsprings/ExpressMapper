using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TicketViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public VenueViewModel Venue { get; set; }
    }
}
