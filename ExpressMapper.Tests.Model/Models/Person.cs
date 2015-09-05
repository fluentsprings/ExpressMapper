using System;

namespace ExpressMapper.Tests.Model.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Person Relative { get; set; }
    }
}
