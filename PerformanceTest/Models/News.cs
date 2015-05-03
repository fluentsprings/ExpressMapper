using System;

namespace PerformanceTest.Models
{
    public class News
    {
        public Guid Id { get; set; }
        public string Provider { get; set; }
        public DateTime StartDate { get; set; }
        public string Url { get; set; }
        public bool IsXml { get; set; }
    }
}
