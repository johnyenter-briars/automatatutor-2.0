using AutomataPDL.PDA.PDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Simulation.DirectSimulation
{
    public static class DirectSimulationRunner<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        public static SimulationPath<A, S> RunSimulation(PDA<A, S> pda, A[] word)
        {
            var acceptanceResult = pda.AcceptsWord(word);

            if (!acceptanceResult.Accepts())
            {
                throw new NoAcceptanceException("the pda does not accept the word, therefore, a simulation is not possible");
            }

            var initialNode = SimulationNode<A, S>.InitialNode(
                new Configuration<A, S>(pda.InitialState, new Word<A>(word), CurrentStack<S>.WithSingleSymbol(pda.FirstStackSymbol)), 
                pda.AcceptanceCondition);
            var frontChain = new List<SimulationNode<A, S>> { initialNode };

            while (frontChain.Count() > 0)
            {
                var nodesAcceptedWord = frontChain.Where(node => node.HasAcceptedWord).ToList();
                if (nodesAcceptedWord.Count() > 0)
                {
                    return SimulationPathFromFinalNode(nodesAcceptedWord.First());
                }

                foreach (var node in frontChain)
                {
                    node.DoStep();
                }

                frontChain = frontChain.SelectMany(node => node.Children).ToList();
            }
            throw new InvalidOperationException("the given pda does not accept the word, therefore a simulation is not possible");
        }

        internal static SimulationPath<A, S> SimulationPathFromFinalNode(SimulationNode<A, S> finalNode)
        {
            var path = new List<SimulationNode<A, S>>() { finalNode };
            var currentNode = finalNode;

            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
                path.Insert(0, currentNode);
            }
            return new SimulationPath<A, S>(path);
        }
    }
}
