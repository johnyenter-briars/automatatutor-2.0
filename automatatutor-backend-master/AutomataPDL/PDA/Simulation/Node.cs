using AutomataPDL.PDA.PDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.PDA.Simulation
{
    public class Node<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal Configuration<A, S> Config { get; }
        internal Transition<A, S> TransitionToHere { get; }

        public Node(Configuration<A, S> config, Transition<A, S> transitionToHere)
        {
            Config = config;
            TransitionToHere = transitionToHere;
        }

        public static Node<A, S> InitialNode(Configuration<A, S> config)
        {
            return new Node<A, S>(config, null);
        }

        public static Node<A, S> ApplyTransitionToParentNode(Transition<A, S> transitionToHere, Node<A, S> parent)
        {
            return new Node<A, S>(Configuration<A, S>.ApplyTransitionToConfiguration(transitionToHere, parent.Config), transitionToHere);
        }

        internal XElement ToXml()
        {
            var transitionXElement = TransitionToHere?.ToXml();
            return new XElement("node", transitionXElement, Config.ToXml());
        }

        public static Node<A, S> NodeInEmptyStackPathBackToFinalStatePath(PDA<A, S> pdaWithFinalState, Node<A, S> node, bool firstNode)
        {
            var oldStackLength = node.Config.Stack.StackSymbols.Count();
            var config = new Configuration<A, S>(
                pdaWithFinalState.States[node.Config.State.Id - 1], 
                node.Config.RemainingWord, 
                new CurrentStack<S>(node.Config.Stack.StackSymbols.Take(oldStackLength - 1)));
            return new Node<A, S>(config, firstNode ? null : node.TransitionToHere);
        }
    }
}
