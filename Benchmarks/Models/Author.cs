using System;
using System.Collections.Generic;

namespace Benchmarks.Models
{
    public class Author
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public List<Article> Articles { get; set; }
    }
}
