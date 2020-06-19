using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.PDA.PDA
{
    public class Configuration<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal State<A, S> State { get; }
        internal Word<A> RemainingWord { get; }
        internal CurrentStack<S> Stack { get; }

        public Configuration(State<A, S> state, Word<A> remainingWord, CurrentStack<S> stack)
        {
            State = state;
            RemainingWord = remainingWord;
            Stack = stack;
        }

        public static Configuration<A, S> ApplyTransitionToConfiguration(Transition<A, S> transition, Configuration<A, S> configurationBefore)
        {
            return new Configuration<A, S>(transition.Target, 
                Word<A>.ApplyTransitionToWord<S>(transition, configurationBefore.RemainingWord), 
                CurrentStack<S>.ApplyTransitionToStack<A>(transition, configurationBefore.Stack));
        }

        internal IEnumerable<Transition<A, S>> GetEnterableTransitions()
        {
            return State.Transitions.Where(t =>
            {
                bool transtionHasCorrectSymbolIn = t.SymbolIn.IsEmpty() || (!RemainingWord.IsEmpty() && t.SymbolIn.GetSymbol().Equals(RemainingWord.Symbols.First()));
                bool transitionHasCorrectStackSymbol = !Stack.IsEmpty() && Stack.StackSymbols.First().Equals(t.StackSymbolIn);
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
