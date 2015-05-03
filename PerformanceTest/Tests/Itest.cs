namespace PerformanceTest.Tests
{
    public interface ITest
    {
        void RunTest(int count);
        void RunTestManualForEach(int count);
        void PrintResults();
    }
}
