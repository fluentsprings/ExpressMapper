using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class BrandViewModel : IEquatable<BrandViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }


        public bool Equals(BrandViewModel other)
        {
            return Id == other.Id && Name == other.Name;
        }
    }
}
