using System;
using System.Diagnostics;
using AutomataPDL.PDA.PDA;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPDL.PDATest
{
    [TestClass]
    public class PDAPerformanceTest
    {
        [TestMethod]
        public void TestPerformance()
        {
            var pdaWithShorterSequence = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZXY");
            pdaWithShorterSequence.AddState(1, false);
            pdaWithShorterSequence.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZZZZZZZZZZZ");
            pdaWithShorterSequence.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pdaWithShorterSequence.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pdaWithShorterSequence.AddTransition().From(1).To(1).Read('a').Pop('Z').Push();
            pdaWithShorterSequence.CreateRunner();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Assert.IsTrue(pdaWithShorterSequence.AcceptsWord("a").Accepts());
            stopwatch.Stop();
            Console.WriteLine("Runtime for shorter:" + stopwatch.Elapsed);

            var pdaWithLongerSequence = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZXY");
            pdaWithLongerSequence.AddState(1, false);
            pdaWithLongerSequence.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZZZZZZZZZZZZZ");
            pdaWithLongerSequence.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pdaWithLongerSequence.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pdaWithLongerSequence.AddTransition().From(1).To(1).Read('a').Pop('Z').Push();
            pdaWithLongerSequence.CreateRunner();

            var stopwatch2 = new Stopwatch();
            stopwatch2.Start();
            Assert.IsTrue(pdaWithLongerSequence.AcceptsWord("a").Accepts());
            stopwatch2.Stop();
            Console.WriteLine("Runtime for longer:" + stopwatch2.Elapsed);
        }
    }
}
