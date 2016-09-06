using System;
using System.Reflection;
using NUnit.Common;
using NUnitLite;

namespace ExpressMapper.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //#if DNX451
            //            new AutoRun().Execute(args);
            //#else
            var writter = new ExtendedTextWrapper(Console.Out);
            new AutoRun(typeof (Program).GetTypeInfo().Assembly).Execute(args, writter, Console.In);

            Console.ReadLine();
//#endif
        }
    }
}
