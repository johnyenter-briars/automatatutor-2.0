using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.SDA;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.DPDA
{
    public class DPDAInNormalFormToSDAConverter<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        public class DeterminisedSDAResult
        {
            public DeterminisedSDA<A, TripleStackSymbol<S>> sda { get; private set; }
            public IEnumerable<StackSymbolSequence<TripleStackSymbol<S>>> InitialConfigSet1 { get; private set; }
            public IEnumerable<StackSymbolSequence<TripleStackSymbol<S>>> InitialConfigSet2 { get; private set; }

            public DeterminisedSDAResult(DeterminisedSDA<A, TripleStackSymbol<S>> sda, IEnumerable<StackSymbolSequence<TripleStackSymbol<S>>> initialConfig1, IEnumerable<StackSymbolSequence<TripleStackSymbol<S>>> initialConfig2)
            {
                this.sda = sda;
                InitialConfigSet1 = initialConfig1;
                InitialConfigSet2 = initialConfig2;
            }
        }

        /// <summary>
        /// converts the given DPDA to a determinised SDA. The both given stack symbols are converted to the corresponding configuration-sets
        /// in the SDA
        /// </summary>
        /// <param name="dpda"></param>
        /// <param name="initialStackSymbols1"></param>
        /// <param name="initialStackSymbols2"></param>
        /// <returns></returns>
        public static DeterminisedSDAResult ToDeterminisedSDA(PDA<A, S> dpda, S initialStackSymbols1, S initialStackSymbols2)
        {
            var sdaResult = ToSDAInNormalForm(dpda);

            var allStackSymbols = DictionaryFromStackSymbols(sdaResult.sda.AllStackSymbols);
            var epsilonStackSymbols = DictionaryFromStackSymbols(sdaResult.EpsilonStackSymbols);

            var initialConfig1 = ConvertInitialConfigurationToSDAConfiguration(allStackSymbols, epsilonStackSymbols, dpda, initialStackSymbols1);
            var initialCinfig2 = ConvertInitialConfigurationToSDAConfiguration(allStackSymbols, epsilonStackSymbols, dpda, initialStackSymbols2);

            var res = DeterminisedSDA<A, TripleStackSymbol<S>>.FromSDAInNormalForm(sdaResult.sda);

            return new DeterminisedSDAResult(res, initialConfig1, initialCinfig2);
        }

        private static Dictionary<string, TripleStackSymbol<S>> DictionaryFromStackSymbols(IEnumerable<TripleStackSymbol<S>> stackSymbols)
        {
            var allStackSymbols = new Dictionary<string, TripleStackSymbol<S>>();
            foreach (var s in stackSymbols)
            {
                allStackSymbols.Add(s.GetKey(), s);
            }
            return allStackSymbols;
        }

        private static IEnumerable<StackSymbolSequence<TripleStackSymbol<S>>> ConvertInitialConfigurationToSDAConfiguration(Dictionary<string, TripleStackSymbol<S>> allStackSymbols, Dictionary<string, TripleStackSymbol<S>> epsilonStackSymbols, PDA<A, S> dpda, S initialStackSymbol)
        {
            var res = new List<StackSymbolSequence<TripleStackSymbol<S>>>();
            foreach (var s in dpda.States)
            {
                var tripleKey = TripleStackSymbol<S>.GetKey(dpda.InitialState.Id, initialStackSymbol, s.Key);
                if (allStackSymbols.ContainsKey(tripleKey))
                {
                    var triple = allStackSymbols[tripleKey];
                    res.Add(new StackSymbolSequence<TripleStackSymbol<S>>(triple));
                }
                else if (epsilonStackSymbols.ContainsKey(tripleKey))
                {
                    res.Add(new StackSymbolSequence<TripleStackSymbol<S>>());
                }
            }

            Assertion.Assert(res.All(s => s.StackSequence.Length > 0) || res.Count == 1, "if a configuration set contains the empty sequence (Epsilon), then it is only admissible if this is the only element");

            return res;
        }

        public class SDAResult
        {
            public IEnumerable<TripleStackSymbol<S>> EpsilonStackSymbols { get; private set; }
            public SDA<A, TripleStackSymbol<S>> sda { get; private set; }

            public SDAResult(IEnumerable<TripleStackSymbol<S>> epsilonTransitions, SDA<A, TripleStackSymbol<S>> sda)
            {
                EpsilonStackSymbols = epsilonTransitions;
                this.sda = sda;
            }
        }

        /// <summary>
        /// creates a SDA (a PDA with onyl one state) in normal form out of a DPDA in normal form
        /// </summary>
        /// <param name="dpda">DPDA in normal form</param>
        /// <returns></returns>
        public static SDAResult ToSDAInNormalForm(PDA<A, S> dpda)
        {
            Assertion.Assert(dpda.Deterministic, "the given PDA is not deterministic");
            Assertion.Assert(dpda.AcceptanceCondition.IsEmptyStack(), "the given PDA has not acceptance condition empty stack");
            Assertion.Assert(dpda.NormalForm, "the given DPDA has no normal form, but this is required");

            var allNewStackSymbols = CreateStackSymbols(dpda);
            var res = new SDA<A, TripleStackSymbol<S>>(allNewStackSymbols.Values);
            AddTransitions(res, dpda, allNewStackSymbols);
            var epsilonSymbols = RemoveEpsilonSymbolsFromRightHandSide(res, dpda, allNewStackSymbols);
            NormalizeSDA(res);

            return new SDAResult(epsilonSymbols, res);
        }

        /// <summary>
        /// removes all redundant stack symbols, that means all symbols that only accept the empty language (when they are interpreted as configuration);
        /// this algorithm is very similar to the one that removes non-producing Nonterminals from a CFG
        /// </summary>
        /// <param name="sda"></param>
        private static void NormalizeSDA(SDA<A, TripleStackSymbol<S>> sda)
        {
            var stackSymbolsThatPushNothing = sda.Transitions.Where(t => t.StackSymbolsWritten.Length == 0).Select(t => t.StackSymbolIn).ToList();
            var notRedundantSymbols = new HashSet<TripleStackSymbol<S>>(stackSymbolsThatPushNothing);

            var lastCount = 0;
            do
            {
                lastCount = notRedundantSymbols.Count;
                var newNotRedundantStackSymbols = sda.Transitions.Where(t => t.StackSymbolsWritten.All(s => notRedundantSymbols.Contains(s))).Select(t => t.StackSymbolIn).ToList();
                notRedundantSymbols.UnionWith(newNotRedundantStackSymbols);
            }
            while (lastCount < notRedundantSymbols.Count);

            var redundantSymbols = sda.AllStackSymbols.Except(notRedundantSymbols);
            sda.RemoveStackSymbols(redundantSymbols);
        }

        private static IEnumerable<TripleStackSymbol<S>> RemoveEpsilonSymbolsFromRightHandSide(SDA<A, TripleStackSymbol<S>> sda, PDA<A, S> dpda, Dictionary<string, TripleStackSymbol<S>> stackSymbols)
        {
            var epsilonSymbols = new List<TripleStackSymbol<S>>();

            var transitions = dpda.States.SelectMany(s => s.Value.Transitions).ToList();

            foreach (var t in transitions.Where(t => t.SymbolIn.IsEmpty()))
            {
                var epsilonSymbol = stackSymbols[TripleStackSymbol<S>.GetKey(t.Origin.Id, t.StackSymbolIn, t.Target.Id)];

                epsilonSymbols.Add(epsilonSymbol);

                foreach (var sdaTransition in sda.Transitions.Where(sdaTransition => sdaTransition.StackSymbolsWritten.Contains(epsilonSymbol)))
                {
                    sdaTransition.RemoveEpsilonSymbol(epsilonSymbol);
                }
            }

            return epsilonSymbols;
        }

        private static void AddTransitions(SDA<A, TripleStackSymbol<S>> sda, PDA<A, S> dpda, Dictionary<string, TripleStackSymbol<S>> stackSymbols)
        {
            var transitions = dpda.States.SelectMany(s => s.Value.Transitions).ToList();

            foreach (var t in transitions.Where(t => !t.SymbolIn.IsEmpty()))
            {
                AddTransition(t, sda, dpda, stackSymbols);
            }
        }

        private static void AddTransition(Transition<A, S> transition, SDA<A, TripleStackSymbol<S>> sda, PDA<A, S> dpda, Dictionary<string, TripleStackSymbol<S>> stackSymbols)
        {
            switch (transition.StackSymbolsWritten.Count())
            {
                case 0:
                    AddTransitionPushingNone(transition, sda, stackSymbols);
                    break;
                case 1:
                    AddTransitionPushingOne(transition, sda, dpda, stackSymbols);
                    break;
                case 2:
                    AddTransitionPushingTwo(transition, sda, dpda, stackSymbols);
                    break;
                default:
                    Assertion.Assert(false, "A DPDA in normal form should push two stack symbols at most");
                    break;
            }
        }

        private static void AddTransitionPushingTwo(Transition<A, S> transition, SDA<A, TripleStackSymbol<S>> sda, PDA<A, S> dpda, Dictionary<string, TripleStackSymbol<S>> stackSymbols)
        {
            foreach (var r in dpda.States)
            {
                foreach (var p in dpda.States)
                {
                    var stackSymbolIn = stackSymbols[TripleStackSymbol<S>.GetKey(transition.Origin.Id, transition.StackSymbolIn, r.Key)];
                    var stackSymbolsOut = new TripleStackSymbol<S>[]
                    {
                        stackSymbols[TripleStackSymbol<S>.GetKey(transition.Target.Id, transition.StackSymbolsWritten[0], p.Key)],
                        stackSymbols[TripleStackSymbol<S>.GetKey(p.Key, transition.StackSymbolsWritten[1], r.Key)]
                    };

                    sda.AddTransition(transition.SymbolIn.GetSymbol(), stackSymbolIn, stackSymbolsOut);
                }
            }
        }

        private static void AddTransitionPushingOne(Transition<A, S> transition, SDA<A, TripleStackSymbol<S>> sda, PDA<A, S> dpda, Dictionary<string, TripleStackSymbol<S>> stackSymbols)
        {
            foreach (var r in dpda.States)
            {
                var stackSymbolIn = stackSymbols[TripleStackSymbol<S>.GetKey(transition.Origin.Id, transition.StackSymbolIn, r.Key)];
                var stackSymbolsOut = new TripleStackSymbol<S>[]
                {
                    stackSymbols[TripleStackSymbol<S>.GetKey(transition.Target.Id, transition.StackSymbolsWritten.First(), r.Key)]
                };
                sda.AddTransition(transition.SymbolIn.GetSymbol(), stackSymbolIn, stackSymbolsOut);
            }
        }

        private static void AddTransitionPushingNone(Transition<A, S> transition, SDA<A, TripleStackSymbol<S>> sda, Dictionary<string, TripleStackSymbol<S>> stackSymbols)
        {
            var stackSymbolIn = stackSymbols[TripleStackSymbol<S>.GetKey(transition.Origin.Id, transition.StackSymbolIn, transition.Target.Id)];
            sda.AddTransition(transition.SymbolIn.GetSymbol(), stackSymbolIn);
        }

        private static Dictionary<string, TripleStackSymbol<S>> CreateStackSymbols(PDA<A, S> dpda)
        {
            var res = new Dictionary<string, TripleStackSymbol<S>>();

            foreach (var state1 in dpda.States)
            {
                foreach (var stackSymbol in dpda.AllStackSymbols)
                {
                    foreach (var state2 in dpda.States)
                    {
                        res.Add(TripleStackSymbol<S>.GetKey(state1.Key, stackSymbol, state2.Key),
                            new TripleStackSymbol<S>(state1.Key, stackSymbol, state2.Key));
                    }
                }
            }

            return res;
        }
    }
}
