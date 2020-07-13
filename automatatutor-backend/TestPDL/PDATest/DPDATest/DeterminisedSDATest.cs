using System;
using System.Collections.Generic;
using System.Linq;
using AutomataPDL.PDA.DPDA;
using AutomataPDL.PDA.SDA;
using AutomataPDL.PDA.DPDA.DPDAEquivalence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPDL.PDATest.DPDATest
{
    [TestClass]
    public class DeterminisedSDATest
    {
        [TestMethod]
        public void FromSDATest()
        {
            var sda = new SDA<char, char>("ZAB");
            sda.AddTransition('a', 'Z', "ZZ");
            sda.AddTransition('a', 'Z', "ZZ");
            sda.AddTransition('a', 'Z', "AZ");
            sda.AddTransition('b', 'A');
            sda.AddTransition('b', 'A', "B");
            sda.AddTransition('c', 'B');
            sda.AddTransition('d', 'Z');

            var exp = new DeterminisedSDA<char, char>("ZAB");
            exp.AddTransition('a', 'Z', new StackSymbolSequenceSet<char>(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("ZZ"), new StackSymbolSequence<char>("AZ") }));
            exp.AddTransition('b', 'A', new StackSymbolSequenceSet<char>(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>(""), new StackSymbolSequence<char>("B") }));
            exp.AddTransition('c', 'B', new StackSymbolSequenceSet<char>(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("")}));
            exp.AddTransition('d', 'Z', new StackSymbolSequenceSet<char>(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("")}));

            var act = DeterminisedSDA<char, char>.FromSDAInNormalForm(sda);

            Assert.IsTrue(exp.Equals(act));

            Assert.IsTrue(act.ShortestWordsOfStackSymbols['Z'].SequenceEqual("d"));
            Assert.IsTrue(act.ShortestWordsOfStackSymbols['B'].SequenceEqual("c"));
            Assert.IsTrue(act.ShortestWordsOfStackSymbols['A'].SequenceEqual("b"));
        }
    }
}
