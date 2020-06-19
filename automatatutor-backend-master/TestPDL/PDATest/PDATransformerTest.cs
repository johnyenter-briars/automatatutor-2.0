using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutomataPDL.PDA.PDA;

namespace TestPDL.PDATest
{
    [TestClass]
    public class PDATransformerTest
    {
        [TestMethod]
        public void MergeDPDAsWithEmptyStackTest()
        {
            var dpda1 = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'A', false, "ABC", 'Z');
            dpda1.AddState(1, false);
            dpda1.AddState(2, false);
            dpda1.AddState(3, false);
            dpda1.AddState(4, false);
            dpda1.AddState(5, false);
            dpda1.AddState(6, false);
            dpda1.AddState(7, false);

            dpda1.AddTransition().From(0).To(1).Read().Pop('A').Push("AAA");
            dpda1.AddTransition().From(1).To(2).Read('a').Pop('A').Push();
            dpda1.AddTransition().From(2).To(3).Read('a').Pop('A').Push();
            dpda1.AddTransition().From(3).To(4).Read().Pop('A').Push("BBCC");
            dpda1.AddTransition().From(4).To(4).Read('b').Pop('B').Push();
            dpda1.AddTransition().From(4).To(4).Read('c').Pop('C').Push();

            dpda1.AddTransition().From(1).To(5).Read().Pop('B').Push();
            dpda1.AddTransition().From(2).To(6).Read().Pop('B').Push("B");
            dpda1.AddTransition().From(6).To(7).Read().Pop('B').Push("B");
            dpda1.AddTransition().From(7).To(6).Read().Pop('B').Push("B");


            var dpda2 = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'A', false, "ABC", 'Z');
            dpda2.AddState(1, false);
            dpda2.AddState(2, false);
            dpda2.AddState(3, false);
            dpda2.AddState(4, false);
            
            dpda2.AddTransition().From(0).To(1).Read().Pop('A').Push("AAA");
            dpda2.AddTransition().From(1).To(2).Read('a').Pop('A').Push();
            dpda2.AddTransition().From(2).To(3).Read('a').Pop('A').Push();
            dpda2.AddTransition().From(3).To(4).Read().Pop('A').Push("BBCC");
            dpda2.AddTransition().From(4).To(4).Read('b').Pop('B').Push();
            dpda2.AddTransition().From(4).To(4).Read('c').Pop('C').Push();

            var mergedDPDAForSimulationOf1 = PDATransformer<char, char>.MergeDPDAsWithEmptyStack(dpda1, dpda2, 'X', 'Y');

            dpda1.CreateRunner();
            mergedDPDAForSimulationOf1.CreateRunner();

            var equalityResult1 = new PDAEqualityResult<char, char>(dpda1, mergedDPDAForSimulationOf1, "abc", 10, 400000, 3000);
            Assert.IsTrue(equalityResult1.AreEqual);


            var mergedDPDAForSimulationOf2 = PDATransformer<char, char>.MergeDPDAsWithEmptyStack(dpda2, dpda1, 'X', 'Y');

            dpda2.CreateRunner();
            mergedDPDAForSimulationOf2.CreateRunner();

            var equalityResult2 = new PDAEqualityResult<char, char>(dpda2, mergedDPDAForSimulationOf2, "abc", 10, 400000, 3000);
            Assert.IsTrue(equalityResult2.AreEqual);
        }

        [TestMethod]
        public void ToDPDAWithEmptyStackTest()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.FinalState(), true, 'Z', false, "ZYX", 'B');

            dpda.AddState(1, false);
            dpda.AddState(2, true);

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            dpda.AddTransition().From(0).To(1).Read('x').Pop('Z').Push("Z");
            dpda.AddTransition().From(0).To(1).Read('x').Pop('X').Push("X");
            dpda.AddTransition().From(0).To(1).Read('x').Pop('Y').Push("Y");

            dpda.AddTransition().From(1).To(1).Read('a').Pop('X').Push("");
            dpda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push("");

            dpda.AddTransition().From(1).To(2).Read().Pop('Z').Push("Z");

            var exp = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'B', false, "BZYX");

            exp.AddState(1, false);
            exp.AddState(2, false);
            exp.AddState(3, false);
            exp.AddState(4, false);

            exp.AddTransition().From(0).To(1).Read().Pop('B').Push("ZB");

            exp.AddTransition().From(1).To(1).Read('a').Pop('Z').Push("XZ");
            exp.AddTransition().From(1).To(1).Read('a').Pop('X').Push("XX");
            exp.AddTransition().From(1).To(1).Read('a').Pop('Y').Push("XY");
            exp.AddTransition().From(1).To(1).Read('b').Pop('Z').Push("YZ");
            exp.AddTransition().From(1).To(1).Read('b').Pop('X').Push("YX");
            exp.AddTransition().From(1).To(1).Read('b').Pop('Y').Push("YY");

            exp.AddTransition().From(1).To(2).Read('x').Pop('Z').Push("Z");
            exp.AddTransition().From(1).To(2).Read('x').Pop('X').Push("X");
            exp.AddTransition().From(1).To(2).Read('x').Pop('Y').Push("Y");

            exp.AddTransition().From(2).To(2).Read('a').Pop('X').Push("");
            exp.AddTransition().From(2).To(2).Read('b').Pop('Y').Push("");

            exp.AddTransition().From(2).To(3).Read().Pop('Z').Push("Z");

            exp.AddTransition().From(3).To(4).Read('$').Pop('Z').Push();
            exp.AddTransition().From(3).To(4).Read('$').Pop('X').Push();
            exp.AddTransition().From(3).To(4).Read('$').Pop('Y').Push();
            exp.AddTransition().From(3).To(4).Read('$').Pop('B').Push();

            exp.AddTransition().From(4).To(4).Read().Pop('Y').Push();
            exp.AddTransition().From(4).To(4).Read().Pop('X').Push();
            exp.AddTransition().From(4).To(4).Read().Pop('Z').Push();
            exp.AddTransition().From(4).To(4).Read().Pop('B').Push();

            var act = PDATransformer<char, char>.ToDPDAWithEmptyStack(dpda, '$');

            Assert.IsTrue(act.Equals(exp));

            act.CreateRunner();

            Assert.IsFalse(act.AcceptsWord("").Accepts());
            Assert.IsFalse(act.AcceptsWord("ax").Accepts());
            Assert.IsFalse(act.AcceptsWord("aaxa").Accepts());
            Assert.IsFalse(act.AcceptsWord("xa").Accepts());
            Assert.IsFalse(act.AcceptsWord("axaa").Accepts());
            Assert.IsFalse(act.AcceptsWord("ax$").Accepts());
            Assert.IsFalse(act.AcceptsWord("aaxa$").Accepts());
            Assert.IsFalse(act.AcceptsWord("xa$").Accepts());
            Assert.IsFalse(act.AcceptsWord("axaa$").Accepts());
            Assert.IsFalse(act.AcceptsWord("$").Accepts());

            Assert.IsTrue(act.AcceptsWord("x$").Accepts());
            Assert.IsTrue(act.AcceptsWord("axa$").Accepts());
            Assert.IsTrue(act.AcceptsWord("aaxaa$").Accepts());
            Assert.IsTrue(act.AcceptsWord("aaaxaaa$").Accepts());   
        }

        [TestMethod]
        public void ToDPDAWithAddedAlphabetSymbolTest()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'Z', false, "ZYX");

            dpda.AddState(1, false);
            dpda.AddState(2, false);

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            dpda.AddTransition().From(0).To(1).Read('x').Pop('Z').Push("Z");
            dpda.AddTransition().From(0).To(1).Read('x').Pop('X').Push("X");
            dpda.AddTransition().From(0).To(1).Read('x').Pop('Y').Push("Y");

            dpda.AddTransition().From(1).To(1).Read('a').Pop('X').Push("");
            dpda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push("");

            dpda.AddTransition().From(1).To(2).Read().Pop('Z').Push();

            var act = PDATransformer<char, char>.ToDPDAWithAddedAlphabetSymbol(dpda, '$');

            act.CreateRunner();

            Assert.IsFalse(act.AcceptsWord("").Accepts());
            Assert.IsFalse(act.AcceptsWord("ax").Accepts());
            Assert.IsFalse(act.AcceptsWord("aaxa").Accepts());
            Assert.IsFalse(act.AcceptsWord("xa").Accepts());
            Assert.IsFalse(act.AcceptsWord("axaa").Accepts());
            Assert.IsFalse(act.AcceptsWord("ax$").Accepts());
            Assert.IsFalse(act.AcceptsWord("aaxa$").Accepts());
            Assert.IsFalse(act.AcceptsWord("xa$").Accepts());
            Assert.IsFalse(act.AcceptsWord("axaa$").Accepts());
            Assert.IsFalse(act.AcceptsWord("$").Accepts());

            Assert.IsTrue(act.AcceptsWord("x$").Accepts());
            Assert.IsTrue(act.AcceptsWord("axa$").Accepts());
            Assert.IsTrue(act.AcceptsWord("aaxaa$").Accepts());
            Assert.IsTrue(act.AcceptsWord("aaaxaaa$").Accepts());
        }
    }
}
