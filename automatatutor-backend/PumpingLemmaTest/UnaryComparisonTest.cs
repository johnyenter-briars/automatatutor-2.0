using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpingLemma;

namespace PumpingLemmaTest
{
    [TestClass]
    public class UnaryComparisonTest
    {
        static List<String> alphabet = new string[] { "a", "b" }.ToList();
        ArithmeticLanguage lang1 = ArithmeticLanguage.FromTextDescriptions(alphabet, "a^n b^n", "n >= 4 && n < 9");
        ArithmeticLanguage lang2 = ArithmeticLanguage.FromTextDescriptions(alphabet, "a^n b^n", "n > 2 && n >= 7");
        ArithmeticLanguage lang3 = ArithmeticLanguage.FromTextDescriptions(alphabet, "a^n b^k", "k > 4 && n < 9 && k == 9");
        ArithmeticLanguage lang4 = ArithmeticLanguage.FromTextDescriptions(alphabet, "a^n b^l", "8 < 3*n && 4 <= 2*l && 2*l <= 9");

        [TestMethod]
        public void TestUnaryComparisonReduction()
        {
            var dic = lang1.getReducedUnaryConstraints();
            Assert.AreEqual("3 < n < 9", dic[VariableType.Variable("n")].ToString());

            dic = lang2.getReducedUnaryConstraints();
            Assert.AreEqual("n > 6", dic[VariableType.Variable("n")].ToString());

            dic = lang3.getReducedUnaryConstraints();
            Assert.AreEqual("n < 9", dic[VariableType.Variable("n")].ToString());
            Assert.AreEqual("k == 9", dic[VariableType.Variable("k")].ToString());

            dic = lang4.getReducedUnaryConstraints();
            Assert.AreEqual("n > 2", dic[VariableType.Variable("n")].ToString());
            Assert.AreEqual("1 < l < 5", dic[VariableType.Variable("l")].ToString());
        }
    }
}
