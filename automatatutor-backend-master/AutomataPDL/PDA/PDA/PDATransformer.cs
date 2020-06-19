using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDA
{
    /// <summary>
    /// provides transform operations from a PDA to a new PDA with sepcific characteristics
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="S"></typeparam>
    public static class PDATransformer<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        const string NotDeterministicError = "the {0} pda is not deterministic, but it has to be so.";
        const string AlreadyEmptyStackError = "the given pda already has only the acceptance-condition 'empty stack'";
        const string NotEmptyStackError = "the given pda does not have the acceptance-condition 'empty stack'";
        const string InvalidSymbolError = "the given symbol already occurs in a transition of the pda, which is not allowed";

        private static Tuple<PDA<A, S>, int> ToRawPDA(PDA<A, S> pda, Func<int, int> idTransformer, bool deterministic)
        {
            Assertion.Assert(pda.AcceptanceCondition.IsFinalState(), AlreadyEmptyStackError);

            var res = new PDA<A, S>(new AcceptanceCondition.EmptyStack(), deterministic, pda.FurtherStackSymbol, false, pda.AllStackSymbols.Concat(new S[] { pda.FurtherStackSymbol }));
            AddStatesOfOriginalPDA(pda, res, idTransformer);
            var newStateId = AddNewState(res);
            AddTransitionsOfOriginalPDA(pda, res, idTransformer);
            AddInitialTransition(pda, res, idTransformer);
            AddEmptyingTransitions(res, newStateId);
            return new Tuple<PDA<A, S>, int>(res, newStateId);
        }

        /// <summary>
        /// creates a new PDA, that accepts the same language as the given PDA, but with empty stack as acceptance condition; the given PDA is assumed to have final state as acceptance condition
        /// </summary>
        /// <param name="pda"></param>
        /// <returns></returns>
        internal static PDA<A, S> ToPDAWithEmptyStack(PDA<A, S> pda)
        {
            int idTransformer(int id) => id + 1;
            var pdaAndNewStateId = ToRawPDA(pda, idTransformer, false);
            AddTransitionsToEmptyingState(pda, pdaAndNewStateId.Item1, pdaAndNewStateId.Item2, Symbol<A>.EpsilonIn(), idTransformer);
            return pdaAndNewStateId.Item1;
        }

        /// <summary>
        /// transforms the given DPDA d with acceptance condition 'final state' to a DPDA with acceptance condition 'empty stack',
        /// that accepts the language {wa | w \element L(d)}, where "a" is the further alphabet symbol.
        /// The added symbol is necessary to keep the determinism-property of the DPDA
        /// </summary>
        /// <param name="pda">the DPDA with acceptance condition 'final state', that should be transformed</param>
        /// <param name="furtherAlphabetSymbol">the symbol, which is added to every word in the language of the given DPDA</param>
        /// <returns>the transformed DPDA</returns>
        public static PDA<A, S> ToDPDAWithEmptyStack(PDA<A, S> pda, A furtherAlphabetSymbol)
        {
            Assertion.Assert(pda.Deterministic, String.Format(NotDeterministicError, ""));

            int idTransformer(int id) => id + 1;
            var pdaAndNewStateId = ToRawPDA(pda, idTransformer, true);
            AddTransitionsToEmptyingState(pda, pdaAndNewStateId.Item1, pdaAndNewStateId.Item2, Symbol<A>.SymbolIn(furtherAlphabetSymbol), idTransformer);
            return pdaAndNewStateId.Item1;
        }

        /// <summary>
        /// transforms the given DPDA d into a new DPDA e with language L(e) = {wa | w \element L(d)}, where "a" is the added symbol
        /// </summary>
        /// <param name="dpda">the DPDA to transform</param>
        /// <param name="addedSymbol">the symbol, which is added to every word in the language of the given DPDA</param>
        /// <returns>the transformed DPDA</returns>
        public static PDA<A, S> ToDPDAWithAddedAlphabetSymbol(PDA<A, S> dpda, A addedSymbol)
        {
            Assertion.Assert(dpda.Deterministic, NotDeterministicError);
            Assertion.Assert(dpda.AcceptanceCondition.IsEmptyStack(), NotEmptyStackError);
            Assertion.Assert(dpda.States.All(s => s.Value.Transitions.All(t => t.SymbolIn.IsEmpty() || !t.SymbolIn.GetSymbol().Equals(addedSymbol))), InvalidSymbolError);

            var res = new PDA<A, S>(new AcceptanceCondition.EmptyStack(), true, dpda.FurtherStackSymbol, false, dpda.AllStackSymbols.Concat(new S[] { dpda.FurtherStackSymbol }));
            int idTransformer(int id) => id + 1;
            AddStatesOfOriginalPDA(dpda, res, idTransformer);
            AddTransitionsOfOriginalPDA(dpda, res, idTransformer);
            AddInitialTransition(dpda, res, idTransformer);
            AddTransitionsForAddedSymbol(dpda, res, dpda.FurtherStackSymbol, addedSymbol, idTransformer);
            return res;
        }

        /// <summary>
        /// Merges the both given DPDA with acceptance condition 'empty stack' into a new one, that simply contains both DPDAs. The initial state
        /// of the result DPDA has two transitions, one to the initial state of pda1 and one to the initial state of pda2, each of these transitions
        /// reads the corresponding given stack symbol. That means, for simulating pda1 with the merged one you just have to start the simulation 
        /// with the initialStackSymbol1 as start configuration
        /// </summary>
        /// <param name="dpda1">DPDA with empty stack</param>
        /// <param name="dpda2">DPDA with empty stack</param>
        /// <param name="initialStackSymbol1">the stack symbol that signals that the first DPDA should be simulated</param>
        /// <param name="initialStackSymbol2">the stack symbol that signals that the second DPDA should be simulated</param>
        /// <returns>the merged DPDA</returns>
        public static PDA<A, S> MergeDPDAsWithEmptyStack(PDA<A, S> dpda1, PDA<A, S> dpda2, S initialStackSymbol1, S initialStackSymbol2)
        {
            Assertion.Assert(dpda1.Deterministic, NotDeterministicError);
            Assertion.Assert(dpda2.Deterministic, NotDeterministicError);
            Assertion.Assert(dpda1.AcceptanceCondition.IsEmptyStack(), NotEmptyStackError);
            Assertion.Assert(dpda2.AcceptanceCondition.IsEmptyStack(), NotEmptyStackError);
            Assertion.Assert(!initialStackSymbol1.Equals(initialStackSymbol2), "the both initial stack symbols must not be the same");

            var newStackSymbols = dpda1.AllStackSymbols.Concat(dpda2.AllStackSymbols).Concat(new S[] { initialStackSymbol1, initialStackSymbol2 }).Distinct();
            var res = new PDA<A, S>(new AcceptanceCondition.EmptyStack(), true, initialStackSymbol1, false, newStackSymbols);

            int idTransformer1(int id) => id + 1;
            int maxIdOfDPDA1 = dpda1.States.Max(s => s.Key) + 1;
            int idTransformer2(int id) => idTransformer1(id + maxIdOfDPDA1);

            AddStatesOfOriginalPDA(dpda1, res, idTransformer1);
            AddStatesOfOriginalPDA(dpda2, res, idTransformer2);

            AddTransitionsOfOriginalPDA(dpda1, res, idTransformer1);
            AddTransitionsOfOriginalPDA(dpda2, res, idTransformer2);

            res.AddTransition().From(res.InitialState.Id).To(idTransformer1(dpda1.InitialState.Id)).Read().Pop(initialStackSymbol1).Push(new List<S>() { dpda1.FirstStackSymbol});
            res.AddTransition().From(res.InitialState.Id).To(idTransformer2(dpda2.InitialState.Id)).Read().Pop(initialStackSymbol2).Push(new List<S>() { dpda2.FirstStackSymbol});

            return res;
        }

        private static void AddStatesOfOriginalPDA(PDA<A, S> pda, PDA<A, S> newPDA, Func<int, int> transformer)
        {
            foreach (var state in pda.States)
            {
                newPDA.AddState(transformer(state.Key), false);
            }
        }

        private static int AddNewState(PDA<A, S> newPDA)
        {
            var newStateId = newPDA.States.Max(n => n.Key) + 1;
            newPDA.AddState(newStateId, false);
            return newStateId;
        }

        private static void AddTransitionsOfOriginalPDA(PDA<A, S> pda, PDA<A, S> newPDA, Func<int, int> transformer)
        {
            foreach (var state in pda.States)
            {
                var transitions = state.Value.Transitions;
                foreach (var t in transitions)
                {
                    newPDA.AddTransition().From(transformer(state.Key)).To(transformer(t.Target.Id)).Read(t.SymbolIn).Pop(t.StackSymbolIn).Push(t.StackSymbolsWritten);
                }
            }
        }

        private static void AddInitialTransition(PDA<A, S> pda, PDA<A, S> newPDA, Func<int, int> transformer)
        {
            newPDA.AddTransition().From(newPDA.InitialState.Id).To(transformer(pda.InitialState.Id)).Read().Pop(newPDA.FirstStackSymbol).Push(new List<S>() { pda.FirstStackSymbol, newPDA.FirstStackSymbol });
        }

        private static void AddEmptyingTransitions(PDA<A, S> newPDA, int newStateId)
        {
            foreach (var stackSymbol in newPDA.AllStackSymbols)
            {
                newPDA.AddTransition().From(newStateId).To(newStateId).Read().Pop(stackSymbol).Push();
            }
        }

        private static void AddTransitionsToEmptyingState(PDA<A, S> pda, PDA<A, S> newPDA, int newStateId, Symbol<A> symbolIn, Func<int, int> transformer)
        {
            foreach (var finalState in pda.States.Where(s => s.Value.Final).ToList())
            {
                foreach (var stackSymbol in newPDA.AllStackSymbols)
                {
                    newPDA.AddTransition().From(transformer(finalState.Key)).To(newStateId).Read(symbolIn).Pop(stackSymbol).Push();
                }
            }
        }

        private static void AddTransitionsForAddedSymbol(PDA<A, S> pda, PDA<A, S> newPDA, S lowestStackSymbol, A addedSymbol, Func<int, int> transformer)
        {
            foreach (var state in pda.States.ToList())
            {
                var newId = transformer(state.Key);
                newPDA.AddTransition().From(newId).To(newId).Read(Symbol<A>.SymbolIn(addedSymbol)).Pop(lowestStackSymbol).Push();
            }
        }
    }
}
