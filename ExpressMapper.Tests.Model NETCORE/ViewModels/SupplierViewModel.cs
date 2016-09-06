using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class SupplierViewModel : IEquatable<SupplierViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime AgreementDate { get; set; }

        public List<SizeViewModel> Sizes { get; set; }
        public int Rank
        {
            get { return AgreementDate.Subtract(DateTime.Now).TotalDays < 1 ? 2 : 10; }
        }

        public bool Equals(SupplierViewModel other)
        {
            var equals = ((Sizes != null && other.Sizes != null) && Sizes.Count == other.Sizes.Count) || ((other.Sizes == null && Sizes == null));
            if (equals && Sizes != null && other.Sizes != null && Sizes.Where((t, i) => !t.Equals(other.Sizes[i])).Any())
            {
                equals = false;
            }
            return Id == other.Id && Name == other.Name && AgreementDate == other.AgreementDate && Rank == other.Rank && equals;
        }
    }
}
