using NUnit.Framework;

namespace ExpressMapper.Tests
{
    public class BaseTestClass
    {
        [TearDown]
        public void Cleanup()
        {
            Mapper.Reset();
        }
    }
}
