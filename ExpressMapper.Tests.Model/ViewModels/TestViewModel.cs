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
        public decimal? Weight { get; set; }
        public CountryViewModel Country { get; set; }
        public List<SizeViewModel> Sizes { get; set; }
        public DateTime Created { get; set; }
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

            return Id == other.Id && Name == other.Name && Age == other.Age && Weight == other.Weight &&
                   Created == other.Created && ((Country == null && other.Country == null) || Country.Equals(other.Country)) && sizes;
        }
    }
}
