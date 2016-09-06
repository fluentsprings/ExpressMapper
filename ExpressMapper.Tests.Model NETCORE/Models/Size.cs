using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Size
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int? NotNullable { get; set; }
        public int Nullable { get; set; }
        public int SortOrder { get; set; }
        public bool BoolValue { get; set; }

        public bool Equals(Size other)
        {
            return Id == other.Id && Name == other.Name && Alias == other.Alias && NotNullable.GetValueOrDefault() == other.NotNullable.GetValueOrDefault() && Nullable == other.Nullable && SortOrder == other.SortOrder && BoolValue == other.BoolValue;
        }
    }
}
