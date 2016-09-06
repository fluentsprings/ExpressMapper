using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TripCatalogViewModel : IEquatable<TripCatalogViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TripTypeViewModel TripType { get; set; }


        public bool Equals(TripCatalogViewModel other)
        {
            return Id == other.Id && Name == other.Name && TripType.Equals(other.TripType);
        }
    }
}
