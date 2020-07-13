using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.DPDA
{
    public  static class DPDAToNormalFormConverter<A, S> where S : IEquatable<S> where A : IEquatable<A>
    {
        class VirtualConfiguration<U, V> where  U: IEquatable<U> where V : IEquatable<V>
        {
            internal State<U, V> state;
            internal IEnumerable<V> stack;
            internal Transition<U, V> transitionToHere;

            public VirtualConfiguration(State<U, V> state, IEnumerable<V> stack, Transition<U, V> transitionToHere)
            {
                this.state = state;
                this.stack = stack;
                this.transitionToHere = transitionToHere;
            }

            internal IEnumerable<Transition<U, V>> GetEnterableTransitions()
            {
                return state.Transitions.Where(t => t.StackSymbolIn.Equals(stack.First()));
            }

            internal VirtualConfiguration<U, V> EnterEpsilonTransition()
            {
                var enterableEpsilonTransitions = GetEnterableTransitions().Where(t => t.SymbolIn.IsEmpty());
                Assertion.Assert(enterableEpsilonTransitions.Count() <= 1, "in a DPDA, there may be only one epsilon transition per configuration");
                Assertion.Assert(enterableEpsilonTransitions.Count() == 1, "there are no enterbale epsilon-transitions");

                var enterableEpsilonTransition = enterableEpsilonTransitions.First();
                return new VirtualConfiguration<U, V>(enterableEpsilonTransition.Target, enterableEpsilonTransition.StackSymbolsWritten.Concat(stack.Skip(1)).ToList(), enterableEpsilonTransition);
            }

            internal bool IsStartOfNotDecreasingCircle(VirtualConfiguration<U, V> circleEndConfig)
            {
                return circleEndConfig.state.Id == state.Id && stack.Count() <= circleEndConfig.stack.Count();
            }
        }

        /// <summary>
        /// creates a new PDA in normal form: for every transition is the maximum number of pushed stack-symbols 2 
        /// and no Epsilon-Transition does push any stack-symbols
        /// </summary>
        /// <param name="pda"></param>
        /// <returns></returns>
        public static PDA<A, StackSymbolSequence<S>> ToNormalForm(PDA<A, S> pda)
        {
            Assertion.Assert(pda.Deterministic, "the normal form can only be created for deterministic PDAs, but the given is not");

            var res = new PDA<A, StackSymbolSequence<S>>(pda.AcceptanceCondition, true, new StackSymbolSequence<S>(pda.FirstStackSymbol), pda.InitialState.Final, new List<StackSymbolSequence<S>>() { });
            AddStatesToPDA(pda, res);
            AddTransitionsToPDA(pda, res);
            CorrectEpsilonTransitions(res);
            res.HasNormalForm();
            return res;
        }

        public static PDA<A, S> ToDPDAWithNormalEpsilonTransitions(PDA<A, S> dpda)
        {
            Assertion.Assert(dpda.Deterministic, "the epsilon transitions can only be normalized for a deterministic PDA, but the given is not");

            var res = dpda.Clone();
            CorrectEpsilonTransitions(res);
            return res;
        }

        private static void CorrectEpsilonTransitions<U, V>(PDA<U, V> resPDA) where U : IEquatable<U> where V : IEquatable<V>
        {
            var epsilonTransitions = resPDA.States.SelectMany(s => s.Value.Transitions).Where(t => t.SymbolIn.IsEmpty()).ToList();

            while (epsilonTransitions.Count > 0)
            {
                var next = epsilonTransitions[0];
                epsilonTransitions.RemoveAt(0);

                CorrectEpsilonTransition(next, resPDA);
            }
        }

        private static void CorrectEpsilonTransition<U, V>(Transition<U, V> epsilonTransition, PDA<U, V> pda)  where U : IEquatable<U> where V : IEquatable<V>
        {
            var configPath = new List<VirtualConfiguration<U, V>>
            {
                new VirtualConfiguration<U, V>(epsilonTransition.Origin, new List<V>() { epsilonTransition.StackSymbolIn }, null)
            };

            while (true)
            {
                var currentConfig = configPath.Last();

                if (CheckIfStackIsEmpty(pda, epsilonTransition, currentConfig))
                {
                    break;
                }

                if (CheckIfConfigIsStable(pda, epsilonTransition, currentConfig))
                {
                    break;
                }

                if (CheckIfTransitionIsUseless(pda, epsilonTransition, configPath))
                {
                    break;
                }

                configPath.Add(currentConfig.EnterEpsilonTransition());
            }
        }

        /// <summary>
        /// checks whether a circle, that does not decrease the stack, was finished for the second time
        /// </summary>
        /// <param name="pda">DPDA</param>
        /// <param name="epsilonTransition">the initial transition which is examined</param>
        /// <param name="configPath">the path with all configurations</param>
        /// <returns></returns>
        private static bool CheckIfTransitionIsUseless<U, V>(PDA<U, V> pda, Transition<U, V> epsilonTransition, List<VirtualConfiguration<U, V>> configPath) where U : IEquatable<U> where V : IEquatable<V>
        {
            var currentConfig = configPath.Last();
            var potentialCircleStarts = ((IEnumerable<VirtualConfiguration<U, V>>) configPath).Reverse().Skip(1).Where(c => c.IsStartOfNotDecreasingCircle(currentConfig)).ToList();
            return potentialCircleStarts.Any(circleStartConfig =>
            {
                var i = configPath.IndexOf(circleStartConfig);
                var circleTransitions = configPath.Skip(i + 1).Select(c => c.transitionToHere).ToList();
                if (i >= circleTransitions.Count())
                {
                    var potentialLastCircleRun = configPath.Skip(i + 1 - circleTransitions.Count).Take(circleTransitions.Count).Select(c => c.transitionToHere).ToList();
                    if (Enumerable.SequenceEqual(circleTransitions, potentialLastCircleRun))
                    {
                        pda.RemoveTransition(epsilonTransition);
                        return true;
                    }
                    return false;
                }
                return false;
            });
        }

        private static bool CheckIfConfigIsStable<U, V>(PDA<U, V> pda, Transition<U, V> epsilonTransition, VirtualConfiguration<U, V> currentConfig) where U : IEquatable<U> where V : IEquatable<V>
        {
            var enterableTransitions = currentConfig.GetEnterableTransitions();
            var enterableEpsilonTransitions = enterableTransitions.Where(t => t.SymbolIn.IsEmpty()).ToList();
            Assertion.Assert(enterableEpsilonTransitions.Count <= 1, "a DPDA can only have one enterable epsilon transition in each configuration");

            if (enterableEpsilonTransitions.Count == 0)
            {
                pda.RemoveTransition(epsilonTransition);

                foreach (var transition in enterableTransitions)
                {
                    Assertion.Assert(!transition.SymbolIn.IsEmpty(), "there should be no epsilon transitions at this point");
                    var newStack = transition.StackSymbolsWritten.Concat(currentConfig.stack.Skip(1).ToList()).ToList();
                    pda.AddTransition().From(epsilonTransition.Origin.Id).To(transition.Target.Id).Read(transition.SymbolIn).Pop(epsilonTransition.StackSymbolIn).Push(newStack);
                }

                return true;
            }
            return false;
        }

        private static bool CheckIfStackIsEmpty<U, V>(PDA<U, V> pda, Transition<U, V> epsilonTransition, VirtualConfiguration<U, V> currentConfig) where U : IEquatable<U> where V : IEquatable<V>
        {
            if (currentConfig.stack.Count() == 0)
            {
                pda.RemoveTransition(epsilonTransition);
                pda.AddTransition().From(epsilonTransition.Origin.Id).To(currentConfig.state.Id).Read().Pop(epsilonTransition.StackSymbolIn).Push();
                return true;
            }
            return false;
        }

        private static void AddStatesToPDA(PDA<A, S> origin, PDA<A, StackSymbolSequence<S>> target)
        {
            foreach (var state in origin.States.Where(s => s.Key != origin.InitialState.Id))
            {
                target.AddState(state.Key, state.Value.Final);
            }
        }

        private static void AddToCollectionsIfNotAlreadyExisting(Queue<StackSymbolSequence<S>> stackSymbolQueue, PDA<A, StackSymbolSequence<S>> pda, StackSymbolSequence<S> stackSymbol)
        {
            if (!pda.AllStackSymbols.Any(s => s.Equals(stackSymbol)))
            {
                pda.AddStackSymbol(stackSymbol);
                stackSymbolQueue.Enqueue(stackSymbol);
            }
        }

        private static void AddTransitionsToPDA(PDA<A, S> originPDA, PDA<A, StackSymbolSequence<S>> targetPDA)
        {
            var originTransitions = originPDA.States.SelectMany(state => state.Value.Transitions);

            int maximumPushedStackSequenceLength = originTransitions.Max(t => t.StackSymbolsWritten.Length);

            foreach (var outerOrigTransition in originTransitions)
            {
                AddSingleTransitionToPDA(targetPDA, originTransitions, outerOrigTransition);
            }
        }

        private static void AddSingleTransitionToPDA(PDA<A, StackSymbolSequence<S>> targetPDA, IEnumerable<Transition<A, S>> originTransitions, Transition<A, S> outerOrigTransition)
        {
            var stackSymbolQueue = new Queue<StackSymbolSequence<S>>();

            var readStackSymbol = new StackSymbolSequence<S>(outerOrigTransition.StackSymbolIn);
            AddToCollectionsIfNotAlreadyExisting(stackSymbolQueue, targetPDA, readStackSymbol);

            while (stackSymbolQueue.Count > 0)
            {
                AddTransitionsForNewStackSymbol(targetPDA, originTransitions, stackSymbolQueue);
            }
        }

        private static void AddTransitionsForNewStackSymbol(PDA<A, StackSymbolSequence<S>> targetPDA, IEnumerable<Transition<A, S>> originTransitions, Queue<StackSymbolSequence<S>> stackSymbolQueue)
        {
            var next = stackSymbolQueue.Dequeue();
            foreach (var innerOriginTransition in originTransitions.Where(t => next.StackSequence.First().Equals(t.StackSymbolIn)))
            {
                var upperWrittenSymbol = new StackSymbolSequence<S>(innerOriginTransition.StackSymbolsWritten);
                var lowerWrittenSymbl = new StackSymbolSequence<S>(next.StackSequence.Skip(1).ToArray());
                var newStackSymbolsWritten = GetNotEmptyStackSymbols(ArrayOf(upperWrittenSymbol, lowerWrittenSymbl));

                foreach (var s in newStackSymbolsWritten)
                {
                    AddToCollectionsIfNotAlreadyExisting(stackSymbolQueue, targetPDA, s);
                }

                targetPDA.AddTransition().From(innerOriginTransition.Origin.Id).To(innerOriginTransition.Target.Id).Read(innerOriginTransition.SymbolIn).Pop(next).Push(newStackSymbolsWritten);
            }
        }

        private static IEnumerable<StackSymbolSequence<S>> GetNotEmptyStackSymbols(IEnumerable<StackSymbolSequence<S>> stackSymbols)
        {
            return stackSymbols.Where(s => s.StackSequence.Length > 0).ToArray();
        }

        private static Z[] ArrayOf<Z>(params Z[] stackSymbol)
        {
            return stackSymbol;
        }
    }
}
