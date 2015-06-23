using System;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class ItemModelViewModel : IEquatable<ItemModelViewModel>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public SubItemViewModel[] SubItems { get; set; }
        public bool Equals(ItemModelViewModel other)
        {
            var subItems = true;
            if (SubItems != null && other.SubItems != null)
            {
                if (SubItems.Length == other.SubItems.Length)
                {
                    if (SubItems.Where((t, i) => !t.Equals(other.SubItems[i])).Any())
                    {
                        subItems = false;
                    }
                }
            }
            return Id == other.Id && Name == other.Name && subItems;
        }
    }
}
