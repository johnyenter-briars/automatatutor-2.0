using System;
using System.Linq;
using System.Collections.Generic;

namespace AutomataPDL.PDA.PDA
{
    public class State<A, S> : IEquatable<State<A, S>> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal int Id { get; }
        internal bool Final { get; }
        /// <summary>
        /// transitions enterable form this state
        /// </summary>
        internal List<Transition<A, S>> Transitions { get; set; }

        internal State(int id, bool final)
        {
            Id = id;
            Final = final;
            Transitions = new List<Transition<A, S>>();
        }

        internal void AddTransition(State<A, S> target, Symbol<A> symbolIn, S stackSymbolIn, S[] stackSymbolsWritten)
        {
            Transitions.Add(new Transition<A, S>(this, target, symbolIn, stackSymbolIn, stackSymbolsWritten));
        }

        private bool ContainsTransitions(List<Transition<A, S>> otherTransitions)
        {
            return Transitions.All(t => otherTransitions.Any(tOther => t.Equals(tOther)));
        }

        public bool Equals(State<A, S> other)
        {
            return Id == other.Id && Final == other.Final
                && Transitions.Count == other.Transitions.Count
                && Transitions.OrderBy(t => t.Id).SequenceEqual(other.Transitions.OrderBy(t => t.Id));
        }
    }
}
