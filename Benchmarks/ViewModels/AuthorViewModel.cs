using System;
using System.Collections.Generic;

namespace Benchmarks.ViewModels
{
    public class AuthorViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public List<ArticleViewModel> OwnedArticles { get; set; }
    }
}
