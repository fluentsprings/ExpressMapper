using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class SizeViewModel : IEquatable<SizeViewModel>
    {
        private readonly Func<string, string> _fullNameFunc;
        public SizeViewModel() { }
        public SizeViewModel(Func<string, string> fullNameFunc)
        {
            _fullNameFunc = fullNameFunc;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int NotNullable { get; set; }
        public int? Nullable { get; set; }
        public bool BoolValue { get; set; }

        public string Fullname {
            get { return _fullNameFunc == null ? string.Format("{0} - FULL NAME - {1}", Alias, Name) : _fullNameFunc(Name); }
        }
        public int SortOrder { get; set; }
        public bool Equals(SizeViewModel other)
        {
            return Id == other.Id && Name == other.Name && Alias == other.Alias && NotNullable == other.NotNullable && Nullable.GetValueOrDefault() == other.Nullable.GetValueOrDefault() && SortOrder == other.SortOrder && Fullname == other.Fullname && BoolValue == other.BoolValue;
        }
    }
}
