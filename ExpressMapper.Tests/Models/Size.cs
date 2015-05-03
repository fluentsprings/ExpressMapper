using System;

namespace ExpressMapper.Tests.Models
{
    public class Size
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int SortOrder { get; set; }
    }
}
