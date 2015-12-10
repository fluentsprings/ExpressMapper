using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class SpecialGiftViewModel : GiftViewModel, IEquatable<SpecialGiftViewModel>
    {
        public new SpecialPersonViewModel Recipient { get; set; }

        public bool Equals(SpecialGiftViewModel other)
        {
            return Recipient.Equals(other.Recipient) && base.Equals(other);
        }
    }
}
