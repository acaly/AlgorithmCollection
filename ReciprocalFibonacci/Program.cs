using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReciprocalFibonacci
{
    class Program
    {
        static void Test(IEnumerable<int> calculator, int count)
        {
            var memStart = GC.GetTotalMemory(true);
            var clock = Stopwatch.StartNew();
            foreach (var i in calculator.Take(count))
            {
                //Console.Write(i);
            }
            //Console.WriteLine();
            var time = clock.ElapsedMilliseconds;
            var memAlloc = GC.GetTotalMemory(true) - memStart;
            Console.WriteLine("...Calculation finishes for {0}. Time = {1} ms. Memory = {2:0.00} MB.",
                count, time, memAlloc / 1024f / 1024);
        }

        static void Main()
        {
            var number = 500;
            Console.WriteLine("Testing first {0} digits", number);
            Console.WriteLine("1 digits");
            Test(new BigInt_10().Calculate(), number);
            Console.WriteLine("2 digits");
            Test(new BigInt_100().Calculate(2), number);
            Console.WriteLine("16 digits");
            Test(new BigInt_100().Calculate(16), number);
            Console.WriteLine("50 digits");
            Test(new BigInt_1n0().Calculate(50), number);
            Console.WriteLine("100 digits");
            Test(new BigInt_1n0().Calculate(100), number);
            Console.WriteLine("100 digits merge");
            Test(new BigInt_Merge_1n0().Calculate(100), number);
            Console.WriteLine("Dynamic merge");
            Test(new BigInt_Merge_Dyn().Calculate(), number);
            Console.ReadLine();
        }
    }
}
