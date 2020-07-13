using AutomataPDL.CFG;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.PDARunner;
using AutomataPDL.PDA.Simulation;
using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.CFGUtils.CYKTable
{
    /// <summary>
    /// represents a node in a derivation of a word
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class DerivationNode<A> where A : IEquatable<A>
    {
        internal GrammarSymbol Symbol { get; }
        //null for terminals
        readonly Production productionFromHere = null;
        readonly IEnumerable<DerivationNode<A>> children = new List<DerivationNode<A>>();

        public DerivationNode(Exprinal<A> symbol)
        {
            Symbol = symbol;
        }

        public DerivationNode()
        {
            Symbol = null;
        }

        private DerivationNode(Production productionFromHere)
        {
            Symbol = productionFromHere.Lhs;
            this.productionFromHere = productionFromHere;
        }

        public DerivationNode(Production productionFromHere, IEnumerable<DerivationNode<A>> children) : this(productionFromHere)
        {
            Assertion.Assert(productionFromHere.Rhs.Count() == children.Count(), "you have to specify as many derivation nodes as right hand symbols");
            Assertion.Assert(productionFromHere.Rhs.Zip(children, (s, c) => new { rhsSymbol = s, child = c }).All(el => el.child.Symbol.Equals(el.rhsSymbol)), "something went wrong here");

            this.children = children;
        }

        private bool ContainsHelperVariable()
        {
            return Symbol.Name.StartsWith(CFGTo2NFConverter<A, char>.PrefixOfAddedNonterminals);
        }

        private bool ContainsTerminal()
        {
            if (Symbol is Exprinal<char>)
            {
                Assertion.Assert(productionFromHere == null, "for terminals the production must be null");
                return true;
            }
            else
            {
                Assertion.Assert(productionFromHere != null, "for non-terminals the production must not be null");
                return false;
            }
        }

        private IEnumerable<DerivationNode<A>> EliminateHelperVariables()
        {
            if (!ContainsHelperVariable())
            {
                return new List<DerivationNode<A>>() { this };
            }

            return children.SelectMany(c => c.EliminateHelperVariables()).ToList();
        }

        private void ConvertToPDASimulationPath(List<Node<A, char>> nodes, PDA<A, char> pda)
        {
            Assertion.Assert(!ContainsHelperVariable(), "should not be called for a helper variable");

            var rightHandsSideNodes = children.SelectMany(c => c.EliminateHelperVariables()).ToList();

            var parent = nodes.Last();
            var splitted = PDAToCFGConverter<A, char>.SplitNonTerminalId(Symbol.Name);

            Assertion.Assert(parent.Config.State.Id == splitted.Item1, "illegal path");

            var isEpsilonTransition = rightHandsSideNodes.Count == 0 || !rightHandsSideNodes.First().ContainsTerminal();

            var inputLetter = isEpsilonTransition ? Symbol<A>.EpsilonIn() : 
                Symbol<A>.SymbolIn((A)Convert.ChangeType(rightHandsSideNodes.First().Symbol.Name, typeof(A)));
            var stackSymbolIn = splitted.Item2;
            char stackSymbolFromNode(DerivationNode<A> n) => PDAToCFGConverter<A, char>.SplitNonTerminalId(n.Symbol.Name).Item2;
            var stackSymbolsOut = isEpsilonTransition ? rightHandsSideNodes.Select(stackSymbolFromNode).ToList() : rightHandsSideNodes.Skip(1).Select(stackSymbolFromNode) ;

            int targetStateId;
            int getFirstStateIfFromNode(DerivationNode<A> n) => PDAToCFGConverter<A, char>.SplitNonTerminalId(n.Symbol.Name).Item1;

            if (isEpsilonTransition)
            {
                if (rightHandsSideNodes.Count == 0)
                {
                    targetStateId = splitted.Item3;
                }
                else
                {
                    targetStateId = getFirstStateIfFromNode(rightHandsSideNodes.First());
                }
            }
            else
            {
                if (rightHandsSideNodes.Count == 1)
                {
                    targetStateId = splitted.Item3;
                }
                else
                {
                    targetStateId = getFirstStateIfFromNode(rightHandsSideNodes[1]);
                }
            }

            var transition = parent.Config.State.Transitions.First(t => 
            t.Target.Id == targetStateId 
            && t.SymbolIn.Equals(inputLetter) 
            && t.StackSymbolIn.Equals(stackSymbolIn) 
            && t.StackSymbolsWritten.SequenceEqual(stackSymbolsOut));

            nodes.Add(Node<A, char>.ApplyTransitionToParentNode(transition, parent));

            var rightHandSideWithNonterminals = isEpsilonTransition ? rightHandsSideNodes : rightHandsSideNodes.Skip(1);

            foreach (var node in rightHandSideWithNonterminals)
            {
                node.ConvertToPDASimulationPath(nodes, pda);
            }
        }

        //FIXME: create own subclass class for derivation start node
        /// <summary>
        /// converts a derivation of a word in a CFG in 2NF, that was originally created from a PDA, back to a path in this PDA;
        /// this path is generated according to the converting-algorithm of a PDA to a CFG implemented in <see cref="PDAToCFGConverter{A, S}"/>;
        /// if this method is called on a CFG that does not fulfill the described conditions, it produces not useful result
        /// </summary>
        /// <param name="pda">pda</param>
        /// <param name="startSymbol">start symbol of the cfg</param>
        /// <param name="word">word of the derivation</param>
        /// <returns></returns>
        public SimulationPath<A, char> ConvertToPDASimulationPath(PDA<A, char> pda, GrammarSymbol startSymbol, A[] word)
        {
            Assertion.Assert(Symbol == startSymbol, "this method should only be called on the start node; FIXME to make it only availabe there");
            Assertion.Assert(productionFromHere.Rhs.Count() == 1, "the first production should only lead to one element");
            Assertion.Assert(children.Count() == 1, "the start symbol should only have one child");

            var symbol = productionFromHere.Rhs[0].Name;
            var splitted = PDAToCFGConverter<A, char>.SplitNonTerminalId(symbol);

            var firstNode = Node<A, char>.InitialNode(new Configuration<A, char>(pda.States[splitted.Item1],
                new Word<A>(word),
                CurrentStack<char>.WithSingleSymbol(splitted.Item2)));

            var nodes = new List<Node<A, char>>()
            {
                firstNode
            };

            children.First().ConvertToPDASimulationPath(nodes, pda);

            return new SimulationPath<A, char>(nodes);
        }
    }
}
