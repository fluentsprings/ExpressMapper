using System;

namespace PerformanceTest.ViewModels
{
    public class AuthorViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public ArticleViewModel[] OwnedArticles { get; set; }
    }
}
