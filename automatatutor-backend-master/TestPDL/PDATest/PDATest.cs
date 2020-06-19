using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.PDARunnerDirect;

namespace TestPDL.PDATest
{
    [TestClass]
    public class PDATest
    {
        const string stackAlphabet = "XYZ";
        const char furtherSymbol = 'V';

        [TestMethod]
        public void TestPDAWithoutFinalState()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }
        
        [TestMethod]
        public void TestPDAEmptyStack()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }
        
        [TestMethod]
        public void TestPDA1()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("X");
            pda.AddTransition().From(0).To(0).Read().Pop('X').Push("XX");
            pda.AddTransition().From(0).To(1).Read('a').Pop('X').Push();
            pda.AddTransition().From(1).To(2).Read('a').Pop('X').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaab").Accepts());
        }
        
        [TestMethod]
        public void TestPDA2()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, false);
            pda.AddState(5, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(3).Read('a').Pop('Z').Push();
            pda.AddTransition().From(3).To(4).Read('a').Pop('Z').Push();
            pda.AddTransition().From(4).To(5).Read('a').Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaab").Accepts());
        }

        
        [TestMethod]
        public void TestPDA2WithEmptyStack()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, false);
            pda.AddState(5, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(3).Read('a').Pop('Z').Push();
            pda.AddTransition().From(3).To(4).Read('a').Pop('Z').Push();
            pda.AddTransition().From(4).To(5).Read('a').Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA3()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', true, "Z", furtherSymbol);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaab").Accepts());
        }
        
        [TestMethod]
        public void TestPDA5()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', true, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(0).Read('a').Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA6()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(2).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA6Extended()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(0).To(2).Read('a').Pop('Z').Push("X");
            pda.AddTransition().From(1).To(3).Read('a').Pop('Z').Push();
            pda.AddTransition().From(2).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("Z");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA7()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push();
            pda.AddTransition().From(2).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(3).Read().Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA8()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA10()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA9()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(2).To(2).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(2).To(0).Read().Pop('Z').Push("ZZ");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA11()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, false);
            pda.AddState(5, false);
            pda.AddState(6, false);
            pda.AddState(7, false);
            pda.AddState(8, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(0).To(6).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(2).To(3).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(3).To(4).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(4).To(5).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(5).To(0).Read().Pop('Z').Push("ZX");
            pda.AddTransition().From(6).To(7).Read().Pop('Z').Push();
            pda.AddTransition().From(7).To(8).Read('a').Pop('X').Push();
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA12()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZX");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('X').Push("Z");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA13()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, stackAlphabet, furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZY");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZX");
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(2).To(2).Read().Pop('Z').Push();
            pda.AddTransition().From(2).To(2).Read().Pop('X').Push();
            pda.AddTransition().From(2).To(2).Read().Pop('Y').Push("Z");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA14()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(0).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push();
            pda.AddTransition().From(2).To(3).Read().Pop('Z').Push();
            pda.AddTransition().From(3).To(4).Read('a').Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA15()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(0).To(2).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(2).To(2).Read().Pop('Z').Push("ZZ");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA16()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, false);
            pda.AddState(5, false);
            pda.AddState(6, false);
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(2).To(0).Read().Pop('Z').Push("ZX");
            pda.AddTransition().From(0).To(3).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(3).To(0).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(3).To(6).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(6).To(3).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(3).To(4).Read().Pop('Z').Push();
            pda.AddTransition().From(4).To(3).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(4).To(5).Read('a').Pop('X').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA17()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, true);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZX");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(2).Read('a').Pop('X').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA18()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("ZZZZZZZZZZ");
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA19()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read('a').Pop('Z').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsTrue(pda.AcceptsWord("aaaaaaaaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA20()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z", furtherSymbol);
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(1).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaaaaaaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDA21()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, false);
            pda.AddState(5, false);
            pda.AddTransition().From(0).To(3).Read().Pop('Z').Push();
            pda.AddTransition().From(3).To(4).Read().Pop('Z').Push();
            pda.AddTransition().From(4).To(0).Read().Pop('Z').Push("ZX");
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(5).Read().Pop('Z').Push();
            pda.AddTransition().From(5).To(2).Read('a').Pop('X').Push();
            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("").Accepts());
            Assert.IsTrue(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaaaaaaaa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("b").Accepts());
            Assert.IsFalse(pda.AcceptsWord("ab").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aab").Accepts());
        }

        [TestMethod]
        public void TestPDAManyInfLoops()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, false);
            pda.AddState(5, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("XZ");
            pda.AddTransition().From(0).To(0).Read().Pop('X').Push("XX");
            pda.AddTransition().From(1).To(1).Read().Pop('X').Push("XX");
            pda.AddTransition().From(2).To(2).Read().Pop('X').Push("XX");
            pda.AddTransition().From(3).To(3).Read().Pop('X').Push("XX");
            pda.AddTransition().From(4).To(4).Read().Pop('X').Push("XX");
            pda.AddTransition().From(5).To(5).Read().Pop('X').Push("XX");

            pda.AddTransition().From(0).To(1).Read().Pop('X').Push("XX");
            pda.AddTransition().From(1).To(2).Read().Pop('X').Push("XX");
            pda.AddTransition().From(2).To(3).Read().Pop('X').Push("XX");
            pda.AddTransition().From(3).To(4).Read().Pop('X').Push("XX");
            pda.AddTransition().From(4).To(5).Read().Pop('X').Push("XX");
            pda.AddTransition().From(5).To(1).Read().Pop('X').Push("XX");
            pda.AddTransition().From(0).To(3).Read().Pop('X').Push("XX");
            pda.AddTransition().From(2).To(5).Read().Pop('X').Push("XX");
            pda.AddTransition().From(5).To(2).Read().Pop('X').Push("XX");
            pda.CreateRunner();

            Assert.IsFalse(pda.AcceptsWord("").Accepts());
            Assert.IsFalse(pda.AcceptsWord("a").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aa").Accepts());
            Assert.IsFalse(pda.AcceptsWord("aaaa").Accepts());
        }


        /*[TestMethod]
        public void TestPDAOwnAlgo()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZX", furtherSymbol);
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddState(4, false);
            pda.AddState(5, false);
            pda.AddState(6, false);
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(2).To(0).Read().Pop('Z').Push("ZX");
            pda.AddTransition().From(0).To(3).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(3).To(0).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(3).To(6).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(6).To(3).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(3).To(4).Read().Pop('Z').Push();
            pda.AddTransition().From(4).To(3).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(4).To(5).Read('a').Pop('X').Push();

            Assert.IsTrue(PDARunner<char, char>.IsWordAccepted(pda, "".ToCharArray()));
            Assert.IsTrue(PDARunner<char, char>.IsWordAccepted(pda, "a".ToCharArray()));
            Assert.IsFalse(PDARunner<char, char>.IsWordAccepted(pda, "aa".ToCharArray()));
            Assert.IsFalse(PDARunner<char, char>.IsWordAccepted(pda, "aaa".ToCharArray()));
            Assert.IsFalse(PDARunner<char, char>.IsWordAccepted(pda, "aaaa".ToCharArray()));
            Assert.IsFalse(PDARunner<char, char>.IsWordAccepted(pda, "b".ToCharArray()));
            Assert.IsFalse(PDARunner<char, char>.IsWordAccepted(pda, "ab".ToCharArray()));
            Assert.IsFalse(PDARunner<char, char>.IsWordAccepted(pda, "aab".ToCharArray()));
        }*/

        [TestMethod]
        public void TestSlowPDA()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZYX");
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZZZZZZZZZ");
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push();
            pda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push();

            pda.CreateRunner();

            Assert.IsTrue(pda.AcceptsWord("aaaaaaaaaaaaaaa").Accepts());
        }

        [TestMethod]
        public void TestOwnRunner()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z");
            pda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("Z");
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("Z");

            var res = PDARunner<char, char>.IsWordAccepted(pda, "a".ToCharArray());

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void TestDPDARunnerForEmptyStack()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'Z', false, "ZXY");
            dpda.AddState(1, false);

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            dpda.AddTransition().From(0).To(1).Read('v').Pop('Z').Push("Z");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('X').Push("X");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('Y').Push("Y");

            dpda.AddTransition().From(1).To(1).Read('a').Pop('X').Push();
            dpda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push();
            dpda.AddTransition().From(1).To(1).Read().Pop('Z').Push();

            dpda.CreateRunner();

            Assert.IsFalse(dpda.AcceptsWord("").Accepts());
            Assert.IsFalse(dpda.AcceptsWord("ababababababvbabababababab").Accepts());
            Assert.IsTrue(dpda.AcceptsWord("abbabaababbabbbbbaaaaabbbbbabaaabbbabaaababbbabvbabbbabaaababbbaaababbbbbaaaaabbbbbabbabaababba").Accepts());
        }

        [TestMethod]
        public void TestDPDARunnerForFinalState()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.FinalState(), true, 'Z', false, "ZXY");
            dpda.AddState(1, false);
            dpda.AddState(2, true);

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            dpda.AddTransition().From(0).To(1).Read('v').Pop('Z').Push("Z");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('X').Push("X");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('Y').Push("Y");

            dpda.AddTransition().From(1).To(1).Read('a').Pop('X').Push();
            dpda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push();
            dpda.AddTransition().From(1).To(2).Read().Pop('Z').Push("XYZ");

            dpda.CreateRunner();

            Assert.IsFalse(dpda.AcceptsWord("").Accepts());
            Assert.IsFalse(dpda.AcceptsWord("ababababababvbabababababab").Accepts());
            Assert.IsTrue(dpda.AcceptsWord("abbabaababbabbbbbaaaaabbbbbabaaabbbabaaababbbabvbabbbabaaababbbaaababbbbbaaaaabbbbbabbabaababba").Accepts());
            Assert.IsTrue(dpda.AcceptsWord("abavaba").Accepts());
        }

        [TestMethod]
        public void TestDPDARunnerForFinalStateAndEmptyStack()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.FinalStateAndEmptyStack(), true, 'Z', false, "ZXY");
            dpda.AddState(1, false);
            dpda.AddState(2, true);

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            dpda.AddTransition().From(0).To(1).Read('v').Pop('Z').Push("Z");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('X').Push("X");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('Y').Push("Y");

            dpda.AddTransition().From(1).To(1).Read('a').Pop('X').Push();
            dpda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push();
            dpda.AddTransition().From(1).To(2).Read().Pop('Z').Push("");

            dpda.CreateRunner();

            Assert.IsFalse(dpda.AcceptsWord("").Accepts());
            Assert.IsFalse(dpda.AcceptsWord("ababababababvbabababababab").Accepts());
            Assert.IsTrue(dpda.AcceptsWord("abbabaababbabbbbbaaaaabbbbbabaaabbbabaaababbbabvbabbbabaaababbbaaababbbbbaaaaabbbbbabbabaababba").Accepts());
            Assert.IsTrue(dpda.AcceptsWord("abavaba").Accepts());
        }

        [TestMethod]
        public void TestDPDARunnerForEmptyLanguage()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'Z', false, "Z");

            dpda.AddTransition().From(0).To(0).Read().Pop('Z').Push("Z");

            dpda.CreateRunner();

            Assert.IsFalse(dpda.AcceptsWord("").Accepts());
        }

        [TestMethod]
        public void TestDPDARunnerForEmptyLanguage2()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'Z', false, "ZX");
            dpda.AddState(1, false);

            dpda.AddTransition().From(0).To(1).Read().Pop('Z').Push("X");
            dpda.AddTransition().From(1).To(1).Read().Pop('X').Push("Z");
            dpda.AddTransition().From(1).To(0).Read().Pop('Z').Push("X");
            dpda.AddTransition().From(0).To(0).Read().Pop('X').Push("Z");

            dpda.CreateRunner();

            Assert.IsFalse(dpda.AcceptsWord("").Accepts());
        }
    }
}
