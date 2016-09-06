using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels.Structs
{
    public struct ItemViewModel : IEquatable<ItemViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public List<FeatureViewModel> FeatureList { get; set; }
        public bool Equals(ItemViewModel other)
        {
            var equals = ((FeatureList != null && other.FeatureList != null) && FeatureList.Count == other.FeatureList.Count) || ((other.FeatureList == null && FeatureList == null));
            if (equals && FeatureList != null && other.FeatureList != null && FeatureList.Where((t, i) => !t.Equals(other.FeatureList[i])).Any())
            {
                equals = false;
            }
            return Id == other.Id && Name == other.Name && Created == other.Created && equals;
        }
    }
}
