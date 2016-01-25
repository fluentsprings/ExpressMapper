using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Venue Venue { get; set; }
    }
}
