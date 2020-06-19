using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpingLemma;
using System.Xml.Linq;

namespace PumpingLemmaTest
{
    [TestClass]
    public class ArithmeticLanguageTests
    {
        public bool closureFlag;

        void BuildAndExpectFailure(string alphabetText, string languageText, string constraintText)
        {
            try
            {
                Console.WriteLine("Building for (" + alphabetText + "), (" + languageText + "), (" + constraintText + ")");
                ArithmeticLanguage.FromTextDescriptions(alphabetText, languageText, constraintText);
                Assert.Fail();
            }
            catch (PumpingLemmaException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception: " + e.ToString());
                Assert.Fail();
            }
        }
        void BuildAndExpectSuccess(string alphabetText, string languageText, string constraintText)
        {
            try
            {
                ArithmeticLanguage.FromTextDescriptions(alphabetText, languageText, constraintText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e);
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestBuilds()
        {
            BuildAndExpectSuccess("a b", "a^i b^j", "i = j");
            BuildAndExpectSuccess("a  b", "a^i b^j", "i = j");
            BuildAndExpectFailure("a cc b", "a^i b^j", "i = j");
            BuildAndExpectFailure("a b", "a^i b^j", "i = k");
        }

        [TestMethod]
        public void TestToDFA()
        {
            ArithmeticLanguage lang = ArithmeticLanguage.FromTextDescriptions("a b", "a^n b^k", "n == 3 && k == 2");
            var dfa = new StringDFA(lang.ToDFA());
            var alphabet = new HashSet<string>(new string[] { "a", "b" });
            Assert.IsTrue(dfa.Accepts("aaabb"));
            Assert.IsFalse(dfa.Accepts("aaabbbb"));
            Assert.IsFalse(dfa.Accepts("aabbb"));
            Assert.IsFalse(dfa.Accepts("bb"));
            XElement el = dfa.ToXML(new string[] { "a", "b" });

            lang = ArithmeticLanguage.FromTextDescriptions("a b", "a^n b^j", "n < 3 && j > 0");
            dfa = new StringDFA(lang.ToDFA());
            Assert.IsTrue(dfa.Accepts("ab"));
            Assert.IsTrue(dfa.Accepts("aabb"));
            Assert.IsFalse(dfa.Accepts("aa"));
            Assert.IsFalse(dfa.Accepts("aaab"));
            Assert.IsTrue(dfa.Accepts("b"));


            lang = ArithmeticLanguage.FromTextDescriptions("a b", "a^n b^j", "n < 3 && n > 0 && j == 1");
            dfa = new StringDFA(lang.ToDFA());
            Assert.IsTrue(dfa.Accepts("ab"));
            Assert.IsTrue(dfa.Accepts("aab"));
            Assert.IsFalse(dfa.Accepts("aa"));
            Assert.IsFalse(dfa.Accepts("aaab"));
            Assert.IsFalse(dfa.Accepts("b"));
        }
    }
}
