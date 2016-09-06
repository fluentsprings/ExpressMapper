using ExpressMapper.Tests.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.Models
{
    public class TestModel : IEquatable<TestModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int? NotNullable { get; set; }
        public int Nullable { get; set; }
        public decimal? Weight { get; set; }
        public long Height { get; set; }
        public bool BoolValue { get; set; }
        public Country Country { get; set; }
        public List<Size> Sizes { get; set; }
        public DateTime Created { get; set; }
        public string Gender { get; set; }
        public GenderTypes? NullableGender { get; set; }
        public string[] StringCollection { get; set; }
        public string CaseInsensitive { get; set; }

        public bool Equals(TestModel other)
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

            return Id == other.Id && Name == other.Name && Age == other.Age && NotNullable.GetValueOrDefault() == other.NotNullable.GetValueOrDefault() && Nullable == other.Nullable && Weight == other.Weight && BoolValue == other.BoolValue && Gender == other.Gender && NullableGender == other.NullableGender &&
                   Created == other.Created && ((Country == null && other.Country == null) || Country.Equals(other.Country)) && sizes;
        }
    }
}
