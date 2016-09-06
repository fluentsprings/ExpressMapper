using System;
using System.Collections.Generic;
using System.Linq;
using ExpressMapper.Tests.Model.ViewModels.Structs;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class CityViewModel : IEquatable<CityViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<FeatureViewModel> FeaturesList { get; set; }
        public bool Equals(CityViewModel other)
        {
            var featureEquals = ((FeaturesList != null && other.FeaturesList != null) && FeaturesList.Count == other.FeaturesList.Count) || ((other.FeaturesList == null && FeaturesList == null));
            if (featureEquals && FeaturesList != null && other.FeaturesList != null && FeaturesList.Where((t, i) => !t.Equals(other.FeaturesList[i])).Any())
            {
                featureEquals = false;
            }
            return Id == other.Id && Name == other.Name && featureEquals;
        }
    }
}
