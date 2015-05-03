using System;
using System.Threading;
using PerformanceTest.Tests;

namespace PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var simpleStructTest = new SimpleStructTest();
            simpleStructTest.RunTest(100000);
            simpleStructTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            simpleStructTest.RunTest(1000000);
            simpleStructTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);



            var simpleTest = new SimpleTest();
            simpleTest.RunTest(100000);
            simpleTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            simpleTest.RunTest(1000000);
            simpleTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            simpleTest.RunTest(5000000);
            simpleTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            var simpleAssociationTest = new SimpleWithAssociationTest();
            simpleAssociationTest.RunTest(100000);
            simpleAssociationTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            simpleAssociationTest.RunTest(1000000);
            simpleAssociationTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);



            var simpleCollectionTest = new SimpleWithCollectionTest();
            simpleCollectionTest.RunTest(10000);
            simpleCollectionTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            simpleCollectionTest.RunTest(100000);
            simpleCollectionTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);



            var complexTest = new ComplexTest();
            complexTest.RunTest(10000);
            complexTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            complexTest.RunTest(100000);
            complexTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            complexTest.RunTest(400000);
            complexTest.PrintResults();

            GC.Collect(2);
            Thread.Sleep(3);

            Console.ReadLine();
        }
    }
}
