using System;
using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Employee> Employees { get; set; }
    }
}
