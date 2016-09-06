using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Country : IEquatable<Country>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public bool Equals(Country other)
        {
            return Id == other.Id && Name == other.Name && Code == other.Code;
        }
    }
}
