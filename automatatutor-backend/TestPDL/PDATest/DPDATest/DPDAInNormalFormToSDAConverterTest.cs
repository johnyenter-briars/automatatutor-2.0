using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.DPDA;
using AutomataPDL.PDA.SDA;
using System.Linq;
using System.Collections.Generic;

namespace TestPDL.PDATest.DPDATest
{
    [TestClass]
    public class DPDAInNormalFormToSDAConverterTest
    {
        [TestMethod]
        public void ToSDAInNormalFormTest()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'Z', false, "ZA");
            dpda.AddState(1, false);

            dpda.HasNormalForm();

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("AZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('A').Push("AA");
            dpda.AddTransition().From(0).To(1).Read('b').Pop('A').Push("A");
            dpda.AddTransition().From(1).To(1).Read('c').Pop('A').Push();
            dpda.AddTransition().From(1).To(1).Read().Pop('Z').Push();

            var symbols = new Dictionary<string, TripleStackSymbol<char>>()
            {
                {"0A1", new TripleStackSymbol<char>(0, 'A', 1) },
                {"0Z1", new TripleStackSymbol<char>(0, 'Z', 1) },
                {"1A1", new TripleStackSymbol<char>(1, 'A', 1) }
            };

            var exp = new SDA<char, TripleStackSymbol<char>>(symbols.Values);
            exp.AddTransition('a', symbols["0Z1"], new TripleStackSymbol<char>[] { symbols["0A1"] });
            exp.AddTransition('a', symbols["0A1"], new TripleStackSymbol<char>[] { symbols["0A1"], symbols["1A1"] });
            exp.AddTransition('b', symbols["0A1"], new TripleStackSymbol<char>[] { symbols["1A1"] });
            exp.AddTransition('c', symbols["1A1"]);

            var act = DPDAInNormalFormToSDAConverter<char, char>.ToSDAInNormalForm(dpda).sda;

            Assert.IsTrue(exp.Equals(act));

            var actPDA = act.ToPDA(symbols["0Z1"]);
            actPDA.CreateRunner();
            dpda.CreateRunner();

            var equalityResult = new PDAEqualityResult<char, TripleStackSymbol<char>>(dpda, actPDA, "abc", 7, 300000, 3000);
            Assert.IsTrue(equalityResult.AreEqual);

            var actDeterminisedSDA = DeterminisedSDA<char, TripleStackSymbol<char>>.FromSDAInNormalForm(act);
            actDeterminisedSDA.CalculateShortestWordsOfStackSymbols();

            Assert.IsTrue(actDeterminisedSDA.ShortestWordsOfStackSymbols[symbols["1A1"]].SequenceEqual("c"));
            Assert.IsTrue(actDeterminisedSDA.ShortestWordsOfStackSymbols[symbols["0A1"]].SequenceEqual("bc"));
            Assert.IsTrue(actDeterminisedSDA.ShortestWordsOfStackSymbols[symbols["0Z1"]].SequenceEqual("abc"));
        }
    }
}
