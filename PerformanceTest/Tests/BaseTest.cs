using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTest.Tests
{
    public abstract class BaseTest<T, TN> : ITest
    {
        protected int Count { set; get; }
        private Stopwatch AutoMapperStopwatch { get; set; }
        private Stopwatch ExpressMapperStopwatch { get; set; }
        private Stopwatch NativeMapperStopwatch { get; set; }

        public void RunTest(int count)
        {
            Count = count;
            
            InitAutoMapper();
            InitExpressMapper();
            InitNativeMapper();
            Console.WriteLine("Mapping initialization finished");

            var src = GetData();

            AutoMapperStopwatch = Stopwatch.StartNew();
            AutoMapperMap(src);
            AutoMapperStopwatch.Stop();
            Console.WriteLine("Automapper mapping has been finished");

            NativeMapperStopwatch = Stopwatch.StartNew();
            NativeMapperMap(src);
            NativeMapperStopwatch.Stop();
            Console.WriteLine("Native mapping has been finished");

            ExpressMapperStopwatch = Stopwatch.StartNew();
            ExpressMapperMap(src);
            ExpressMapperStopwatch.Stop();
            Console.WriteLine("Expressmapper mapping has been finished");
        }

        protected abstract T GetData();
        protected abstract void InitAutoMapper();
        protected abstract void InitExpressMapper();
        protected abstract void InitNativeMapper();

        protected abstract TN AutoMapperMap(T src);
        protected abstract TN ExpressMapperMap(T src);
        protected abstract TN NativeMapperMap(T src);
        protected abstract string TestName { get; }

        public void RunTestManualForEach(int count)
        {
            throw new NotImplementedException();
        }

        public void PrintResults()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Test results for {0} for collection size of {1}", TestName, Count);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Automapper took {0} ms.", AutoMapperStopwatch.ElapsedMilliseconds);
            Console.WriteLine("Expressmapper took {0} ms.", ExpressMapperStopwatch.ElapsedMilliseconds);
            Console.WriteLine("Native code mapping took {0} ms.", NativeMapperStopwatch.ElapsedMilliseconds);
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
