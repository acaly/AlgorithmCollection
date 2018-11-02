using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitWord
{
    class Program
    {
        interface ITestCase
        {
            IEnumerable<string> GetDict();
            string GetString();
        }

        class StaticTestCase : ITestCase
        {
            public IEnumerable<string> Dict;
            public string String;

            public IEnumerable<string> GetDict() => Dict;
            public string GetString() => String;
            public override string ToString()
            {
                return string.Format("static {{ dict = {0}, str = {1} }}", Dict.ToString(), String);
            }
        }
        
        class RandTestCase1 : StaticTestCase
        {
            public RandTestCase1(int seed1, int seed2, int seed3)
            {
                Dict = MakeDict(seed1, 6000, 6, 20).Concat(MakeDict(seed2, 100, 2, 20));
                String = MakeString(seed3, Dict, 10000);
                _s1 = seed1;
                _s2 = seed2;
                _s3 = seed3;
            }
            private int _s1, _s2, _s3;
            public override string ToString()
            {
                return string.Format("rand1 {{ seeds = [{0}, {1}, {2}] }}", _s1, _s2, _s3);
            }
        }

        static void Main(string[] args)
        {
            Test(new StaticTestCase
            {
                Dict = new[] {
                    "ab",
                    "abb",
                    "bb",
                    "bbbb",
                },
                String = "ab" + new string('b', 2 * 50),
            }, false);
            Test(new StaticTestCase
            {
                Dict = new[] {
                    "abcde",
                    "abd",
                    "ab",
                    "c",
                },
                String = "abc",
            }, false);
            Test(new StaticTestCase
            {
                Dict = new[] {
                    "defgh",
                    "abc",
                    "ab",
                    "cdefg",
                },
                String = "abcdefg",
            }, false);

            var rand = new Random();
            var clock = Stopwatch.StartNew();
            for (long i = 0;; i++)
            {
                Test(new RandTestCase1(rand.Next(), rand.Next(), rand.Next()), false);
                if (clock.ElapsedMilliseconds > 2000)
                {
                    clock.Restart();
                    Console.WriteLine("{0} tests.", i.ToString());
                }
            }
        }

        static void Test(ITestCase test, bool canFail)
        {
            var c = new SimpleBacktracking(test.GetDict());
            var str = test.GetString();
            var r = c.Calculate(str);
            if (canFail && r == null) return;
            if (r == null || string.Concat(r) != str)
            {
                Console.WriteLine("Test failed: {0}", test.ToString());
            }
        }

        static List<string> MakeDict(int seed, int num, long avgLen, int chNum)
        {
            var ret = new List<string>();
            var rand = new Random(seed);
            for (long i = 0; i < num; ++i)
            {
                var ch = rand.Next(chNum);
                ret.Add(new string((char)('a' + ch), 1));
            }
            for (long i = 0; i < num * avgLen - num; ++i)
            {
                var r = rand.Next(num);
                var ch = rand.Next(chNum);
                ret[r] = ret[r] + (char)('a' + ch);
            }
            return ret;
        }

        static string MakeString(int seed, IEnumerable<string> idict, int wordNum)
        {
            var dict = idict.ToList();
            var rand = new Random(seed);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < wordNum; ++i)
            {
                var segment = dict[rand.Next(dict.Count)];
                sb.Append(segment);
            }
            return sb.ToString();
        }
    }
}
