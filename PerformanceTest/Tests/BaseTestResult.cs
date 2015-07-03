using System;
using System.Collections.Generic;
using System.Text;

namespace Benchmarks.Tests
{
    public abstract class BaseTestResult
    {
        public static Dictionary<string, Dictionary<string, Dictionary<int, int>>> StructuredResults = new Dictionary<string, Dictionary<string, Dictionary<int, int>>>();
        protected abstract string TestName { get; }
        protected abstract string Size { get; }
        protected int Count { set; get; }

        protected void AddResults(string mapper, int qty, int ms)
        {
            if (!StructuredResults.ContainsKey(Size))
            {
                StructuredResults.Add(Size, new Dictionary<string, Dictionary<int, int>>());
            }

            if (!StructuredResults[Size].ContainsKey(mapper))
            {
                StructuredResults[Size].Add(mapper, new Dictionary<int, int>());
            }

            StructuredResults[Size][mapper][qty] = ms;
        }

        public static void FormatResults()
        {
            Console.WriteLine("-----------------------------------------------------------------------");
            Console.WriteLine("FormattedResults");

            foreach (var structuredResult in StructuredResults)
            {
                Console.WriteLine("{0} : {{", structuredResult.Key);
                var qty = new StringBuilder();
                foreach (var result in structuredResult.Value)
                {
                    foreach (var res in result.Value)
                    {
                        qty.Append(string.Format("'{0}k',", res.Key / 1000));
                    }
                    break;
                }
                var qt = qty.ToString();
                qt = qt.Remove(qt.LastIndexOf(",", System.StringComparison.Ordinal), 1);
                Console.WriteLine("     qty : [{0}],", qt);
                foreach (var result in structuredResult.Value)
                {
                    var mapperVals = new StringBuilder();
                    foreach (var res in result.Value)
                    {
                        mapperVals.Append(string.Format("{0},", res.Value));
                    }
                    var mapper = mapperVals.ToString();
                    mapper = mapper.Remove(mapper.LastIndexOf(",", System.StringComparison.Ordinal), 1);
                    Console.WriteLine("     {0} : [{1}],", result.Key, mapper);
                }
                Console.WriteLine("},");
            }
        }
    }
}
