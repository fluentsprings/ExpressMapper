using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class EmployeeViewModel : IEquatable<EmployeeViewModel>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<EmployeeViewModel> Employees { get; set; }

        public bool Equals(EmployeeViewModel other)
        {
            var subItems = true;
            if (Employees != null && other.Employees != null)
            {
                if (Employees.Count == other.Employees.Count)
                {
                    if (Employees.Where((t, i) => !t.Equals(other.Employees[i])).Any())
                    {
                        subItems = false;
                    }
                }
            }
            return Id == other.Id && FirstName == other.FirstName && LastName == other.LastName && subItems;
        }
    }
}
