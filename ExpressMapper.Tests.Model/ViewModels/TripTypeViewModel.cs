using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TripTypeViewModel: IEquatable<TripTypeViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Equals(TripTypeViewModel other)
        {
            return Id == other.Id && Name == other.Name;
        }
    }
}
