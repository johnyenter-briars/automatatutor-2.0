using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.PDARunner;
using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Simulation
{
    public static class DPDASimulationRunner<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        private const string noTransitionMessage = "there is no transition that can be entered";
        private const string endlessCircleMessage = "an endless circle has been passed";

        public static SimulationPath<A, S> RunSimulation(PDA<A, S> dpda, A[] word, AcceptanceCondition acceptanceCondition)
        {
            return RunSimulation(dpda, word, acceptanceCondition, new CancellationTokenSource().Token);
        }

        public static SimulationPath<A, S> RunSimulation(PDA<A, S> dpda, A[] word, AcceptanceCondition acceptanceCondition, CancellationToken token)
        {
            Assertion.Assert(dpda.Deterministic, "The given PDA is not deterministic");

            var path = new List<Node<A, S>>() {
                Node<A, S>.InitialNode(
                    new Configuration<A, S>(dpda.InitialState,
                    new Word<A>(word), 
                    new CurrentStack<S>(new List<S>() { dpda.FirstStackSymbol })))
            };

            var configAccepts = acceptanceCondition.IsFinalState() ?
                new Func<Configuration<A, S>, bool>(config => config.State.Final)
                : new Func<Configuration<A, S>, bool>(config => config.Stack.IsEmpty());

            var potentialCircleStartsWithMinStackSoFar = new MultiDictionary<int, NodeWithMinimumStackHeightAfterIt>();
            potentialCircleStartsWithMinStackSoFar.Add(dpda.InitialState.Id, 
                new NodeWithMinimumStackHeightAfterIt(path.Last(), path.Last().Config.Stack.StackSymbols.Count()));

            while (!(path.Last().Config.RemainingWord.IsEmpty() && configAccepts(path.Last().Config)))
            {
                token.ThrowIfCancellationRequested();

                var enterableTransitions = path.Last().Config.GetEnterableTransitions();

                Assertion.Assert(enterableTransitions.Count() <= 1, "The given PDA is not deterministic - fatal error");

                if (enterableTransitions.Count() == 0)
                {
                    return new SimulationPath<A, S>(path, noTransitionMessage);
                }

                path.Add(Node<A, S>.ApplyTransitionToParentNode(enterableTransitions.First(), path.Last()));

                if (enterableTransitions.First().SymbolIn.IsEmpty())
                {
                    if (EndlessCircleWasPassed(potentialCircleStartsWithMinStackSoFar, path.Last()))
                    {
                        return new SimulationPath<A, S>(path, endlessCircleMessage);
                    }

                    var currentNode = path.Last();

                    var currentStackHeight = currentNode.Config.Stack.StackSymbols.Count();
                    potentialCircleStartsWithMinStackSoFar.Add(currentNode.Config.State.Id, 
                        new NodeWithMinimumStackHeightAfterIt(currentNode, currentStackHeight));

                    foreach(var n in potentialCircleStartsWithMinStackSoFar.Values)
                    {
                        n.MinimumStackHeightAfterIt = Math.Min(n.MinimumStackHeightAfterIt, currentStackHeight);
                    }
                }
                else
                {
                    potentialCircleStartsWithMinStackSoFar = new MultiDictionary<int, NodeWithMinimumStackHeightAfterIt>();
                }
            }
            return new SimulationPath<A, S>(path);
        }

        class NodeWithMinimumStackHeightAfterIt
        {
            internal Node<A, S> Node { get; }
            internal int MinimumStackHeightAfterIt { get; set; }

            public NodeWithMinimumStackHeightAfterIt(Node<A, S> node, int minimumStackHeightAfterIt)
            {
                Node = node;
                MinimumStackHeightAfterIt = minimumStackHeightAfterIt;
            }
        }

        private static bool EndlessCircleWasPassed(MultiDictionary<int, NodeWithMinimumStackHeightAfterIt> potentialCircleStartsWithMinStack, Node<A, S> currentNode)
        {
            var potentialCircleStartsWithSameState = potentialCircleStartsWithMinStack.ValuesOfKey(currentNode.Config.State.Id);
            var potentialCirlceStartsWithLessOrEqualStackHeight = potentialCircleStartsWithSameState.Where(cirlceStart => cirlceStart.Node.Config.Stack.StackSymbols.Count() <= currentNode.Config.Stack.StackSymbols.Count());
            return potentialCirlceStartsWithLessOrEqualStackHeight.Any(circleStartNodeWithMinStack =>
            {
                var circleStartNode = circleStartNodeWithMinStack.Node;
                var numberOfMaxPoppedStackSymbols = circleStartNode.Config.Stack.StackSymbols.Count() - circleStartNodeWithMinStack.MinimumStackHeightAfterIt + 1;
                return circleStartNode.Config.Stack.HasSameTopMostSymbolsLike(currentNode.Config.Stack, numberOfMaxPoppedStackSymbols);
            });
        }
    }
}
