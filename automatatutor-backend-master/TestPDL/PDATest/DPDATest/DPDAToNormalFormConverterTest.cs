using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.DPDA;
using System.Collections.Generic;

namespace TestPDL.PDATest.DPDATest
{
    [TestClass]
    public class DPDAToNormalFormConverterTest
    {
        [TestMethod]
        public void TestDPDAToNormalForm()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'A', false, "ABC", 'Z');
            dpda.AddState(1, false);
            dpda.AddState(2, false);
            dpda.AddState(3, false);
            dpda.AddState(4, false);
            dpda.AddState(5, false);
            dpda.AddState(6, false);
            dpda.AddState(7, false);

            dpda.AddTransition().From(0).To(1).Read().Pop('A').Push("AAA");
            dpda.AddTransition().From(1).To(2).Read('a').Pop('A').Push();
            dpda.AddTransition().From(2).To(3).Read('a').Pop('A').Push();
            dpda.AddTransition().From(3).To(4).Read().Pop('A').Push("BBCC");
            dpda.AddTransition().From(4).To(4).Read('b').Pop('B').Push();
            dpda.AddTransition().From(4).To(4).Read('c').Pop('C').Push();

            dpda.AddTransition().From(1).To(5).Read().Pop('B').Push();
            dpda.AddTransition().From(2).To(6).Read().Pop('B').Push("B");
            dpda.AddTransition().From(6).To(7).Read().Pop('B').Push("B");
            dpda.AddTransition().From(7).To(6).Read().Pop('B').Push("B");


            var allStackSymbols = new List<StackSymbolSequence<char>>()
            {
                new StackSymbolSequence<char>("A"),
                new StackSymbolSequence<char>("B"),
                new StackSymbolSequence<char>("C"),
                new StackSymbolSequence<char>("AA"),
                new StackSymbolSequence<char>("AAA"),
                new StackSymbolSequence<char>("BBCC"),
                new StackSymbolSequence<char>("BCC"),
                new StackSymbolSequence<char>("CC"),
            };
            var exp = new PDA<char, StackSymbolSequence<char>>(new AcceptanceCondition.EmptyStack(), true, new StackSymbolSequence<char>('A'), false, allStackSymbols);
            exp.AddState(1, false);
            exp.AddState(2, false);
            exp.AddState(3, false);
            exp.AddState(4, false);
            exp.AddState(5, false);
            exp.AddState(6, false);
            exp.AddState(7, false);

            exp.AddTransition().From(0).To(2).Read('a').Pop(new StackSymbolSequence<char>("A")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("AA") });
            exp.AddTransition().From(0).To(2).Read('a').Pop(new StackSymbolSequence<char>("AA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("AA"), new StackSymbolSequence<char>("A") });
            exp.AddTransition().From(0).To(2).Read('a').Pop(new StackSymbolSequence<char>("AAA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("AA"), new StackSymbolSequence<char>("AA") });
            exp.AddTransition().From(1).To(5).Read().Pop(new StackSymbolSequence<char>("B")).Push();

            exp.AddTransition().From(1).To(2).Read('a').Pop(new StackSymbolSequence<char>("A")).Push();
            exp.AddTransition().From(1).To(2).Read('a').Pop(new StackSymbolSequence<char>("AA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("A") });
            exp.AddTransition().From(1).To(2).Read('a').Pop(new StackSymbolSequence<char>("AAA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("AA") });

            exp.AddTransition().From(2).To(3).Read('a').Pop(new StackSymbolSequence<char>("A")).Push();
            exp.AddTransition().From(2).To(3).Read('a').Pop(new StackSymbolSequence<char>("AA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("A") });
            exp.AddTransition().From(2).To(3).Read('a').Pop(new StackSymbolSequence<char>("AAA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("AA") });

            exp.AddTransition().From(3).To(4).Read('b').Pop(new StackSymbolSequence<char>("A")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("BCC") });
            exp.AddTransition().From(3).To(4).Read('b').Pop(new StackSymbolSequence<char>("AA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("BCC"), new StackSymbolSequence<char>("A") });
            exp.AddTransition().From(3).To(4).Read('b').Pop(new StackSymbolSequence<char>("AAA")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("BCC"), new StackSymbolSequence<char>("AA") });

            exp.AddTransition().From(4).To(4).Read('b').Pop(new StackSymbolSequence<char>("B")).Push();
            exp.AddTransition().From(4).To(4).Read('b').Pop(new StackSymbolSequence<char>("BCC")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("CC") });
            exp.AddTransition().From(4).To(4).Read('b').Pop(new StackSymbolSequence<char>("BBCC")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("BCC") });
            exp.AddTransition().From(4).To(4).Read('c').Pop(new StackSymbolSequence<char>("C")).Push();
            exp.AddTransition().From(4).To(4).Read('c').Pop(new StackSymbolSequence<char>("CC")).Push(new StackSymbolSequence<char>[] { new StackSymbolSequence<char>("C") });


            var act = DPDAToNormalFormConverter<char, char>.ToNormalForm(dpda);

            Assert.IsTrue(exp.Equals(act));

            dpda.CreateRunner();
            exp.CreateRunner();
            act.CreateRunner();

            var equalityResult1 = new PDAEqualityResult<StackSymbolSequence<char>, StackSymbolSequence<char>>(exp, act, "abc", 7, 300000, 3000);
            Assert.IsTrue(equalityResult1.AreEqual);

            var equalityResult2 = new PDAEqualityResult<char, StackSymbolSequence<char>>(dpda, act, "abc", 10, 600000, 3000);

            Assert.IsTrue(equalityResult2.AreEqual);

            //this is actually unnecessary
            Assert.IsTrue(act.AcceptsWord("aabbcc").Accepts());
            Assert.IsTrue(!act.AcceptsWord("aabbc").Accepts());
            Assert.IsTrue(!act.AcceptsWord("aabcc").Accepts());
            Assert.IsTrue(!act.AcceptsWord("abbcc").Accepts());
            Assert.IsTrue(!act.AcceptsWord("").Accepts());
        }

        [TestMethod]
        public void TestRealDPDAToNormalForm()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), true, 'Z', false, "ZYX", 'A');
            dpda.AddState(1, false);

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            dpda.AddTransition().From(0).To(1).Read('x').Pop('Z').Push("Z");
            dpda.AddTransition().From(0).To(1).Read('x').Pop('X').Push("X");
            dpda.AddTransition().From(0).To(1).Read('x').Pop('Y').Push("Y");

            dpda.AddTransition().From(1).To(1).Read('a').Pop('X').Push();
            dpda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push();
            dpda.AddTransition().From(1).To(1).Read().Pop('Z').Push();

            var dpdaInNormalForm = DPDAToNormalFormConverter<char, char>.ToNormalForm(dpda);

            dpda.CreateRunner();
            dpdaInNormalForm.CreateRunner();

            var equalityResult = new PDAEqualityResult<char, StackSymbolSequence<char>>(dpda, dpdaInNormalForm, "abx", 10, 600000, 3000);

            Assert.IsTrue(equalityResult.AreEqual);
        }
    }
}
