using ExpressMapper.Tests.Projections.Context;
using NUnit.Framework;

namespace ExpressMapper.Tests.Projections.Tests
{
    public abstract class BaseTest
    {
        protected ExpressContext Context { get; private set; }

        [SetUp]
        public void Initialize()
        {
            Mapper.Reset();
            Context = new ExpressContext();
            Context.Database.Delete();
            Context.Database.Create();

            Setup();
            Execute();
        }

        [TearDown]
        public void Cleanup()
        {
            Mapper.Reset();
            Context.Database.Delete();
            Context.Dispose();
        }

        protected abstract void Setup();
        protected abstract void Execute();
    }
}
