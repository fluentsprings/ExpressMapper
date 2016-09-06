using System;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TestCollectionViewModel : IEquatable<TestCollectionViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }


        public bool Equals(TestCollectionViewModel other)
        {
            return Id == other.Id && Name == other.Name;
        }
    }
}
