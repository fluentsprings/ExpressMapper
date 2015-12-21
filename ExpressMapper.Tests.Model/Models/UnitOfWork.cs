using System;
using ExpressMapper.Tests.Model.Enums;

namespace ExpressMapper.Tests.Model.Models
{
    public class UnitOfWork
    {
        public Guid Id { get; set; }
        public States State { get; set; }
    }
}
