using ExpressMapper.Tests.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TestViewModel : IEquatable<TestViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int NotNullable { get; set; }
        public int? Nullable { get; set; }
        public decimal? Weight { get; set; }
        public long Height { get; set; }
        public bool BoolValue { get; set; }
        public CountryViewModel Country { get; set; }
        public List<SizeViewModel> Sizes { get; set; }
        public DateTime Created { get; set; }
        public GenderTypes Gender { get; set; }
        public string NullableGender { get; set; }
        public int GenderIndex { get; set; }
        public List<String> StringCollection { get; set; }
        public string CaSeInSeNsItIvE { get; set; }
        public bool Equals(TestViewModel other)
        {
            var sizes = true;
            if (Sizes != null && other.Sizes != null)
            {
                if (Sizes.Count == other.Sizes.Count)
                {
                    if (Sizes.Where((t, i) => !t.Equals(other.Sizes[i])).Any())
                    {
                        sizes = false;
                    }
                }
            }

            return Id == other.Id && Name == other.Name && Age == other.Age && NotNullable == other.NotNullable && Nullable.GetValueOrDefault() == other.Nullable.GetValueOrDefault() && Weight == other.Weight && BoolValue == other.BoolValue && Gender == other.Gender && NullableGender == other.NullableGender && GenderIndex == other.GenderIndex && CaSeInSeNsItIvE == other.CaSeInSeNsItIvE &&
                   Created == other.Created && ((Country == null && other.Country == null) || Country.Equals(other.Country)) && sizes;
        }
    }
}
