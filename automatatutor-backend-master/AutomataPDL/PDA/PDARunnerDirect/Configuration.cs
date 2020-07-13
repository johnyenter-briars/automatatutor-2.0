using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutomataPDL.PDA.PDA;

namespace AutomataPDL.PDA.PDARunnerDirect
{
    internal class Configuration<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal State<A, S> State { get; }
        internal Word<A, S> RemainingWord { get; }
        internal Stack<A, S> Stack { get; }

        internal Configuration(Configuration<A, S> parentConfig, Transition<A, S> transitionToHere) : 
            this(transitionToHere.Target, new Word<A, S>(transitionToHere, parentConfig), new Stack<A, S>(transitionToHere, parentConfig))
        {
        }

        internal Configuration(State<A, S> state, Word<A, S> remainingWord, Stack<A, S> stack)
        {
            State = state;
            RemainingWord = remainingWord;
            Stack = stack;
        }

        internal IEnumerable<Transition<A, S>> GetEnterableTransitions()
        {
            return State.Transitions.Where(t =>
            {
                bool transtionHasCorrectSymbolIn = t.SymbolIn.IsEmpty() || (!RemainingWord.IsEmpty() && t.SymbolIn.GetSymbol().Equals(RemainingWord.First()));
                bool transitionHasCorrectStackSymbol = !Stack.IsEmpty() && Stack.First().Equals(t.StackSymbolIn);
                return transitionHasCorrectStackSymbol && transtionHasCorrectSymbolIn;
            }).ToList();
        }

        internal XElement ToXml()
        {
            return new XElement("config", new XElement("state", State.Id), new XElement("word", RemainingWord.ToString()), new XElement("stack", Stack.ToString()));
        }

        public override string ToString()
        {
            return State.Id + "/" + Stack.ToString() + "/" + RemainingWord.ToString();
        }
    }
}
