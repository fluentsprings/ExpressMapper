using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class CountryViewModel : IEquatable<CountryViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Equals(CountryViewModel other)
        {
            return Id == other.Id && Name == other.Name && Code == other.Code;
        }
    }
}
