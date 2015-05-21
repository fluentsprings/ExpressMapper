using System;

namespace PerformanceTest.ViewModels
{
    public class NewsViewModel
    {
        public Guid Id { get; set; }
        public string Provider { get; set; }
        public DateTime StartDate { get; set; }
        public string Url { get; set; }
        public bool IsXml { get; set; }
    }
}
