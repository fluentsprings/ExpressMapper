using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class FashionProductViewModel : ProductViewModel, IEquatable<FashionProductViewModel>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Equals(FashionProductViewModel other)
        {
            var parentEquals = base.Equals(other);
            return Start == other.Start && End == other.End && parentEquals;
        }
    }
}
