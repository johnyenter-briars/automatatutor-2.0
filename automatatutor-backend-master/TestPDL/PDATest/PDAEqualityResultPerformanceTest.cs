using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using AutomataPDL.PDA.PDA;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPDL.PDATest
{
    [TestClass]
    public class PDAEqualityResultPerformanceTest
    {
        [TestMethod]
        public void TestMethod()
        {
            var pdaMirrored = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZAB");
            pdaMirrored.AddState(1, false);

            pdaMirrored.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("AZ");
            pdaMirrored.AddTransition().From(0).To(0).Read('a').Pop('A').Push("AA");
            pdaMirrored.AddTransition().From(0).To(0).Read('a').Pop('B').Push("AB");
            pdaMirrored.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("BZ");
            pdaMirrored.AddTransition().From(0).To(0).Read('b').Pop('A').Push("BA");
            pdaMirrored.AddTransition().From(0).To(0).Read('b').Pop('B').Push("BB");

            pdaMirrored.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pdaMirrored.AddTransition().From(0).To(1).Read().Pop('A').Push("A");
            pdaMirrored.AddTransition().From(0).To(1).Read().Pop('B').Push("B");
            pdaMirrored.AddTransition().From(0).To(1).Read('a').Pop('Z').Push("Z");
            pdaMirrored.AddTransition().From(0).To(1).Read('a').Pop('A').Push("A");
            pdaMirrored.AddTransition().From(0).To(1).Read('a').Pop('B').Push("B");
            pdaMirrored.AddTransition().From(0).To(1).Read('b').Pop('Z').Push("Z");
            pdaMirrored.AddTransition().From(0).To(1).Read('b').Pop('A').Push("A");
            pdaMirrored.AddTransition().From(0).To(1).Read('b').Pop('B').Push("B");

            pdaMirrored.AddTransition().From(1).To(1).Read('a').Pop('A').Push();
            pdaMirrored.AddTransition().From(1).To(1).Read('b').Pop('B').Push();
            pdaMirrored.AddTransition().From(1).To(1).Read().Pop('Z').Push();

            pdaMirrored.CreateRunner();


            var pdaMirroredEven = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZAB");
            pdaMirroredEven.AddState(1, false);

            pdaMirroredEven.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("AZ");
            pdaMirroredEven.AddTransition().From(0).To(0).Read('a').Pop('A').Push("AA");
            pdaMirroredEven.AddTransition().From(0).To(0).Read('a').Pop('B').Push("AB");
            pdaMirroredEven.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("BZ");
            pdaMirroredEven.AddTransition().From(0).To(0).Read('b').Pop('A').Push("BA");
            pdaMirroredEven.AddTransition().From(0).To(0).Read('b').Pop('B').Push("BB");

            pdaMirroredEven.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pdaMirroredEven.AddTransition().From(0).To(1).Read().Pop('A').Push("A");
            pdaMirroredEven.AddTransition().From(0).To(1).Read().Pop('B').Push("B");
            pdaMirroredEven.AddTransition().From(0).To(1).Read('a').Pop('Z').Push("Z");
            pdaMirroredEven.AddTransition().From(0).To(1).Read('a').Pop('A').Push("A");
            pdaMirroredEven.AddTransition().From(0).To(1).Read('a').Pop('B').Push("B");
            pdaMirroredEven.AddTransition().From(0).To(1).Read('b').Pop('Z').Push("Z");
            pdaMirroredEven.AddTransition().From(0).To(1).Read('b').Pop('A').Push("A");

            pdaMirroredEven.AddTransition().From(1).To(1).Read('a').Pop('A').Push();
            pdaMirroredEven.AddTransition().From(1).To(1).Read('b').Pop('B').Push();
            pdaMirroredEven.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pdaMirroredEven.AddTransition().From(1).To(1).Read('a').Pop('Z').Push();

            pdaMirroredEven.CreateRunner();

            var s = new Stopwatch();
            var results = new Dictionary<int, List<Tuple<bool, TimeSpan>>>();
            for (int i = 1; i <= 15; i++)
            {
                results[i] = new List<Tuple<bool, TimeSpan>>();
            }

            for (int j = 0; j < 1; j++) //TODO: count until 10 for a good average
            {
                for (int i = 1; i <= 15; i++)
                {
                    s.Restart();
                    var eq = new PDAEqualityResult<char, char>(pdaMirrored, pdaMirroredEven, "ab", i, 65535, 20000);
                    s.Stop();
                    results[i].Add(new Tuple<bool, TimeSpan>(eq.AreEqual, s.Elapsed));
                }
            }

            foreach(var t in results)
            {
                var equal = t.Value.All(v => v.Item1);
                var time = t.Value.Average(v => v.Item2.TotalMilliseconds);
                Console.WriteLine(string.Format("Maximum word length: {0}; Equal: {1}; Time: {2}", t.Key, equal, time));
            }
        }

        [TestMethod]
        public void TestMaximumNumberOfWords()
        {
            var pda1 = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZXY");
            pda1.AddState(1, false);
            pda1.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            pda1.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            pda1.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            pda1.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            pda1.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            pda1.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            pda1.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda1.AddTransition().From(0).To(1).Read().Pop('X').Push("X");
            pda1.AddTransition().From(0).To(1).Read().Pop('Y').Push("Y");

            pda1.AddTransition().From(1).To(1).Read('c').Pop('X').Push();
            pda1.AddTransition().From(1).To(1).Read('d').Pop('Y').Push();
            pda1.AddTransition().From(1).To(1).Read().Pop('Z').Push();

            var pda2 = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZXY");
            pda2.AddState(1, false);
            pda2.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            pda2.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            pda2.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            pda2.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            pda2.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            pda2.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            pda2.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda2.AddTransition().From(0).To(1).Read().Pop('X').Push("X");
            pda2.AddTransition().From(0).To(1).Read().Pop('Y').Push("Y");

            pda2.AddTransition().From(1).To(1).Read('c').Pop('X').Push();
            pda2.AddTransition().From(1).To(1).Read('d').Pop('Y').Push();
            pda2.AddTransition().From(1).To(1).Read('a').Pop('Z').Push();

            pda1.CreateRunner();
            pda2.CreateRunner();

            var watch = new Stopwatch();

            watch.Start();
            var res = new PDAEqualityResult<char, char>(pda1, pda2, "abcd", 10, 400000, 60000);
            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);

            Assert.IsFalse(res.AreEqual);
        }
    }
}
