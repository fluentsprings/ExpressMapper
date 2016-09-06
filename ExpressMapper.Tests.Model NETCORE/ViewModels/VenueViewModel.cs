using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class VenueViewModel
    {
        public VenueViewModel(string name)
        {
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
