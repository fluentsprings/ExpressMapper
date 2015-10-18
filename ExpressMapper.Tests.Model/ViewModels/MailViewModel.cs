using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class MailViewModel : IEquatable<MailViewModel>
    {
        public string From { get; set; }

        public ContactViewModel Contact { get; set; }

        public bool Equals(MailViewModel other)
        {
            return From == other.From && (Contact == null || Contact.Equals(other.Contact));
        }
    }
}
