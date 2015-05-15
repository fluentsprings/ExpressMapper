using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class TestItemViewModel
    {
        public TestCollectionViewModel[] Array { get; set; }
        public ICollection<TestCollectionViewModel> Collection { get; set; }
        public IList<TestCollectionViewModel> List { get; set; }
        public IEnumerable<TestCollectionViewModel> Enumerable { get; set; }
        public IQueryable<TestCollectionViewModel> Queryable { get; set; }
    }
}
