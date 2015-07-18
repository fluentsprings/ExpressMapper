using System;

namespace Benchmarks.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
