using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.DPDA.DPDAEquivalence
{
    public class StackSymbolSequenceSet<S> :IEquatable<StackSymbolSequenceSet<S>> where S : IEquatable<S>
    {
        internal IEnumerable<StackSymbolSequence<S>> StackSequenceSet { get; }

        public StackSymbolSequenceSet(IEnumerable<StackSymbolSequence<S>> stackSequenceSet)
        {
            StackSequenceSet = stackSequenceSet.Distinct(); //FIXME: does Distinct work??
        }

        public StackSymbolSequenceSet()
        {
            StackSequenceSet = new StackSymbolSequence<S>[] { };
        }

        public StackSymbolSequenceSet(StackSymbolSequence<S> stackSequence)
        {
            StackSequenceSet = new List<StackSymbolSequence<S>>() { stackSequence };
        }

        public StackSymbolSequenceSet<S> Multiply(StackSymbolSequence<S> stackSequence)
        {
            var res = StackSequenceSet.Select(s => new StackSymbolSequence<S>(s.StackSequence.Concat(stackSequence.StackSequence).ToList()));
            return new StackSymbolSequenceSet<S>(res);
        }

        public StackSymbolSequenceSet<S> Multiply(StackSymbolSequenceSet<S> stackSequenceSet)
        {
            var res = stackSequenceSet.StackSequenceSet.SelectMany(s => Multiply(s).StackSequenceSet).ToList();
            return new StackSymbolSequenceSet<S>(res);
        }

        public static StackSymbolSequenceSet<S> Flatten(IEnumerable<StackSymbolSequenceSet<S>> stackSymbolSequenceSets)
        {
            return new StackSymbolSequenceSet<S>(stackSymbolSequenceSets.SelectMany(s => s.StackSequenceSet).ToList());
        }

        internal HeadTailForm<S> Get1HeadTailForm()
        {
            //FIXME: sum up same heads (and corresponding tails), so that a valid head/tail-form is ensured
            var heads = StackSequenceSet.Select(s => new StackSymbolSequence<S>(s.StackSequence.Take(1).ToList())).ToList();
            var tails = StackSequenceSet.Select(s => new StackSymbolSequence<S>(s.StackSequence.Skip(1).ToList())).ToList();
            return new HeadTailForm<S>(new StackSymbolSequenceSet<S>(heads), new StackSymbolSequenceSet<S>(tails));
        }

        internal IEnumerable<int> GetLengths()
        {
            return StackSequenceSet.Select(s => s.StackSequence.Count()).ToList();
        }

        internal bool IsEmptySet()
        {
            return StackSequenceSet.Count() == 0;
        }

        internal bool IsEpsilonSet()
        {
            var res = StackSequenceSet.All(s => s.IsEpsilon());
            if (res)
            {
                Assertion.Assert(StackSequenceSet.Count() == 1, "the stack symbol sequence set has to be distinct");
            }
            return res;
        }

        public bool Equals(StackSymbolSequenceSet<S> other)
        {
            //FIXME: either use HashSet or order StackSequenceSet before SequenceEqual
            //FIXME: HashSet has probably better performance than ordering
            return StackSequenceSet.OrderBy(s => s.Id).SequenceEqual(other.StackSequenceSet.OrderBy(s => s.Id));
        }

        public override string ToString()
        {
            return String.Join("+", StackSequenceSet.OrderBy(s => s.Id).Select(s => s.Id));
        }

        /// <summary>
        /// tries to bring this set and the other set into head/tail-form, so that both have the same tails
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal Tuple<HeadTailForm<S>> TryToGetHeadTailForm(StackSymbolSequenceSet<S> other)
        {
            //FIXME: replace this whole function by another algo, as it will not work like this
            var ownSetWithInvsereOrder = OrderByStackSequencesInverse();
            var otherWithInverseOrder = other.OrderByStackSequencesInverse();
            var setsOrderByLength = new List<StackSymbolSequenceSet<S>>() { ownSetWithInvsereOrder, otherWithInverseOrder }.OrderBy(s => s.StackSequenceSet.Count()).ToList();

            var headsOfSmallerSet = new List<StackSymbolSequence<S>>();
            var headsOfLongerSet = new List<StackSymbolSequence<S>>();
            var tails = new List<StackSymbolSequence<S>>();

            for (int i = 0; i < setsOrderByLength.First().StackSequenceSet.Count(); i++)
            {
                var firstStackSequence = setsOrderByLength[0].StackSequenceSet.ElementAt(i);
                var secondStackSequence = setsOrderByLength[1].StackSequenceSet.ElementAt(i);

                var tail = firstStackSequence.GetLongestPossibleCommonTail(secondStackSequence);
                headsOfSmallerSet.Add(firstStackSequence.GetHeadWithRespectToTail(tail));
                headsOfLongerSet.Add(secondStackSequence.GetHeadWithRespectToTail(tail));
                tails.Add(tail);
            }

            var tailsOrderedByLength = tails.OrderByDescending(t => t.StackSequence.Count());

            var smallerLentgh = setsOrderByLength[0].StackSequenceSet.Count();
            var remaining = setsOrderByLength[1].StackSequenceSet.Count() - smallerLentgh;
            for (int i = 0; i < remaining; i++)
            {
                var next = setsOrderByLength[1].StackSequenceSet.ElementAt(smallerLentgh + i);
                var tail = tailsOrderedByLength.FirstOrDefault(t => next.EndsWith(t));
                //TODO: ...
            }
            return null; //FIXME
        }

        /// <summary>
        /// important: the stack sequences are not re-ordered, they are only order within the set
        /// </summary>
        /// <returns></returns>
        internal StackSymbolSequenceSet<S> OrderByStackSequencesInverse()
        {
            return new StackSymbolSequenceSet<S>(StackSequenceSet.OrderBy(s => new StackSymbolSequence<S>(((IEnumerable<S>)s.StackSequence).Reverse().ToList()).Id));
        }
    }
}
