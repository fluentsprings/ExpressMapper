using System;
using ExpressMapper.Tests.Model.Enums;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class UnitOfWorkViewModel
    {
        public Guid Id { get; set; }
        public AnotherStates State { get; set; }
    }
}
