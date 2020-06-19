using AutomataPDL.PDA.PDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.PDA.Simulation.DirectSimulation
{
    public class SimulationNode<A, S> : Node<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal SimulationNode<A, S> Parent { get; }
        internal IEnumerable<SimulationNode<A, S>> Children { get; private set; }
        internal bool HasAcceptedWord { get; private set; } = false;
        private AcceptanceCondition acceptanceCondition;

        public SimulationNode(Configuration<A, S> config, Transition<A, S> transitionToHere, SimulationNode<A, S> parent, AcceptanceCondition acceptanceCondition) : base(config, transitionToHere)
        {
            Parent = parent;
            this.acceptanceCondition = acceptanceCondition;
            CheckIfWordIsAccepted();
        }

        internal static SimulationNode<A, S> InitialNode(Configuration<A, S> config, AcceptanceCondition acceptanceCondition)
        {
            return new SimulationNode<A, S>(config, null, null, acceptanceCondition);
        }

        internal static SimulationNode<A, S> ApplyTransitionToParentSimulationNode(Transition<A, S> transitionToHere, SimulationNode<A, S> nodeBefore)
        {
            return new SimulationNode<A, S>(Configuration<A, S>.ApplyTransitionToConfiguration(transitionToHere, nodeBefore.Config), transitionToHere, nodeBefore, nodeBefore.acceptanceCondition);
        }

        private void CheckIfWordIsAccepted()
        {
            HasAcceptedWord = Config.RemainingWord.IsEmpty() && (acceptanceCondition.IsEmptyStack() && Config.Stack.IsEmpty() || acceptanceCondition.IsFinalState() && Config.State.Final);
        }

        internal void DoStep()
        {
            Children = Config.GetEnterableTransitions().Select(t => ApplyTransitionToParentSimulationNode(t, this)).ToList();
        }
    }
}
