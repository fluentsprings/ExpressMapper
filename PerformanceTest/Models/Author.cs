using System;
using System.Collections.Generic;

namespace PerformanceTest.Models
{
    public class Author
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public IEnumerable<Article> Articles { get; set; }
    }
}
