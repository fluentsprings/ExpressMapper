using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.Models
{
    public class TestItem
    {
        public TestCollection[] Array { get; set; }
        public ICollection<TestCollection> Collection { get; set; }
        public IList<TestCollection> List { get; set; }
        public IEnumerable<TestCollection> Enumerable { get; set; }
        public IQueryable<TestCollection> Queryable { get; set; }
    }
}
