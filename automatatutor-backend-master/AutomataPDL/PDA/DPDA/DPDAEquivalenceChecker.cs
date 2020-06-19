using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;
using AutomataPDL.PDA.SDA;

namespace AutomataPDL.PDA.DPDA
{
    public static class DPDAEquivalenceChecker<A, S> where S : IEquatable<S> where A : IEquatable<A>
    {
        //TODO: put error strings to one class (or to resources)
        const string NotDeterministicError = "the {0} pda is not deterministic, but it has to be so.";
        const string FurtherAlphabetSymbolInvalidError = "the given further alphabet symbol already occurs in the transitions of the PDAs";

        internal static bool CheckEquivalence(PDA<A, S> pda1, PDA<A, S> pda2, S initialStackSymbol1, S initialStackSymbol2, A furtherAlphabetSymbol)
        {
            Assertion.Assert(pda1.Deterministic, String.Format(NotDeterministicError, "first"));
            Assertion.Assert(pda2.Deterministic, String.Format(NotDeterministicError, "second"));

            bool pdaDoesNotHaveSymbol(PDA<A, S> pda, A symbol) => pda.States.All(s => s.Value.Transitions.All(t => t.SymbolIn.IsEmpty() || !t.SymbolIn.GetSymbol().Equals(symbol)));
            Assertion.Assert(pdaDoesNotHaveSymbol(pda1, furtherAlphabetSymbol) && pdaDoesNotHaveSymbol(pda2, furtherAlphabetSymbol), FurtherAlphabetSymbolInvalidError);

            //TODO: the PDAs have both acceptance conditions, check also the equivalence of this pda with itself (and the other acceptance condition)
            var convertedPDAs = ConvertDPDAsToEmptyStack(pda1, pda2, furtherAlphabetSymbol);
            var mergedDPDA = PDATransformer<A, S>.MergeDPDAsWithEmptyStack(convertedPDAs.Item1, convertedPDAs.Item2, initialStackSymbol1, initialStackSymbol2);
            var mergedDPDAInNormalForm = DPDAToNormalFormConverter<A, S>.ToNormalForm(mergedDPDA);

            var initialStackSymbol1NormalForm = GetStackSymbol(mergedDPDAInNormalForm, initialStackSymbol1);
            var initialStackSymbol2NormalForm = GetStackSymbol(mergedDPDAInNormalForm, initialStackSymbol2);

            var determinisedSDAResult = DPDAInNormalFormToSDAConverter<A, StackSymbolSequence<S>>.ToDeterminisedSDA(mergedDPDAInNormalForm, initialStackSymbol1NormalForm, initialStackSymbol2NormalForm);


            //TODO: create in the FromPDA-method already the partition (see Fact 3.2 in the paper) if it is necessary; 
            //maybe remove the own step of the creation of the SDA and integrate this into the determinisedSDA, 
            //as the partition depends on the DPDA that the SDA is created of

            return false; //FIXME
        }

        private static StackSymbolSequence<S> GetStackSymbol(PDA<A, StackSymbolSequence<S>> pda, S stackSymbol)
        {
            var res = pda.AllStackSymbols.Where(s => s.StackSequence.SequenceEqual(new S[] { stackSymbol }));
            Assertion.Assert(res.Count() <= 1, "Illegal state: the stack symbol exists twice");
            return res.First();
        }

        private static Tuple<PDA<A, S>, PDA<A, S>> ConvertDPDAsToEmptyStack(PDA<A, S> pda1, PDA<A, S> pda2, A furtherAlphabetSymbol)
        {
            if (pda1.AcceptanceCondition.IsEmptyStack() && pda2.AcceptanceCondition.IsEmptyStack())
            {
                return new Tuple<PDA<A, S>, PDA<A, S>>(pda1, pda2);
            }

            if (pda1.AcceptanceCondition.IsFinalState() && pda2.AcceptanceCondition.IsEmptyStack())
            {
                var newPda1 = PDATransformer<A, S>.ToDPDAWithEmptyStack(pda1, furtherAlphabetSymbol);
                var newPda2 = PDATransformer<A, S>.ToDPDAWithAddedAlphabetSymbol(pda2, furtherAlphabetSymbol);
                return new Tuple<PDA<A, S>, PDA<A, S>>(newPda1, newPda2);
            }

            if (pda1.AcceptanceCondition.IsEmptyStack() && pda2.AcceptanceCondition.IsFinalState())
            {
                var newPda1 = PDATransformer<A, S>.ToDPDAWithAddedAlphabetSymbol(pda1, furtherAlphabetSymbol);
                var newPda2 = PDATransformer<A, S>.ToDPDAWithEmptyStack(pda2, furtherAlphabetSymbol);
                return new Tuple<PDA<A, S>, PDA<A, S>>(newPda1, newPda2);
            }

            if (pda1.AcceptanceCondition.IsFinalState() && pda2.AcceptanceCondition.IsFinalState())
            {
                var newPda1 = PDATransformer<A, S>.ToDPDAWithEmptyStack(pda1, furtherAlphabetSymbol);
                var newPda2 = PDATransformer<A, S>.ToDPDAWithEmptyStack(pda2, furtherAlphabetSymbol);
                return new Tuple<PDA<A, S>, PDA<A, S>>(newPda1, newPda2);
            }

            throw new InvalidOperationException("the given pdas have wrong configuration");
        }
    }
}
