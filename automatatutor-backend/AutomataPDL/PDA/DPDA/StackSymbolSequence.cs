using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.DPDA
{
    public class StackSymbolSequence<S> : IEquatable<StackSymbolSequence<S>> where S : IEquatable<S>
    {
        /// <summary>
        /// if StackSequence is empty, Epsilon is represented
        /// </summary>
        internal S[] StackSequence { get; }

        public StackSymbolSequence()
        {
            StackSequence = new S[] { };
        }

        public StackSymbolSequence(S[] stackSequence)
        {
            StackSequence = stackSequence;
        }

        public StackSymbolSequence(IEnumerable<S> stackSequence)
        {
            StackSequence = stackSequence.ToArray();
        }

        public StackSymbolSequence(S stackSymbol)
        {
            StackSequence = new S[] { stackSymbol };
        }

        public bool Equals(StackSymbolSequence<S> other)
        {
            return StackSequence.SequenceEqual(other.StackSequence);
        }

        public bool IsEpsilon()
        {
            return StackSequence.Count() == 0;
        }

        public bool EndsWith(StackSymbolSequence<S> tail)
        {
            return StackSequence.Count() >= tail.StackSequence.Count()
                && new StackSymbolSequence<S>(StackSequence.Skip(StackSequence.Count() - tail.StackSequence.Count()).ToList()).Equals(tail);
        }

        public StackSymbolSequence<S> GetHeadWithRespectToTail(StackSymbolSequence<S> tail)
        {
            Assertion.Assert(EndsWith(tail), "the given tail has to be a tail of this stack sequence");
            return new StackSymbolSequence<S>(StackSequence.Take(StackSequence.Count() - tail.StackSequence.Count()).ToList());
        }

        //TODO: test for this
        public StackSymbolSequence<S> GetLongestPossibleCommonTail(StackSymbolSequence<S> other)
        {
            var reverseZipped = StackSequence.Reverse().Zip(other.StackSequence.Reverse(), (first, second) => new { first, second });
            var tail = reverseZipped.TakeWhile(el => el.first.Equals(el.second)).Select(el => el.first).Reverse().ToList();
            return new StackSymbolSequence<S>(tail);
        }

        /// <summary>
        /// source: https://stackoverflow.com/a/10567544
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return StackSequence.Aggregate(0, (acc, s) => {
                var newAcc = acc ^ s.GetHashCode();
                return (newAcc << 7) | (newAcc >> (32 - 7));
            });
        }

        public String Id
        {
            get
            {
                return String.Join(",", StackSequence);
            }
        }

        public override string ToString()
        {
            return String.Join("", StackSequence);
        }
    }
}