using System;

namespace ExpressMapper.Tests.Projections.ViewModel
{
    public class SizeViewModel : IEquatable<SizeViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Equals(SizeViewModel other)
        {
            return Id == other.Id && Name == other.Name && Code == other.Code;
        }
    }
}
