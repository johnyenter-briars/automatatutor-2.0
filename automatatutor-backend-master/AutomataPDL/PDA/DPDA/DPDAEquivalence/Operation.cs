using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.DPDA.DPDAEquivalence
{
    internal abstract class Operation<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal abstract bool HasBALType(BAL.BalType balType);

        internal abstract class BAL : Operation<A, S>
        {
            internal IEnumerable<StackSymbolSequenceSet<S>> Bottoms { get; }

            internal BAL(IEnumerable<StackSymbolSequenceSet<S>> bottoms)
            {
                Bottoms = bottoms;
            }

            internal abstract BalType GetBALType();

            internal static Func<Node<A, S>, StackSymbolSequenceSet<S>> GetSideSelector(BalType balType)
            {
                switch(balType)
                {
                    case BalType.BAL_L: return BAL_L.SideSelector;
                    case BalType.BAL_R: return BAL_R.SideSelector;
                    default: throw new InvalidOperationException("illegal bal type");
                }
            }

            internal static Func<Node<A, S>, StackSymbolSequenceSet<S>> GetOppositeSideSelector(BalType balType)
            {
                switch(balType)
                {
                    case BalType.BAL_L: return BAL_R.SideSelector;
                    case BalType.BAL_R: return BAL_L.SideSelector;
                    default: throw new InvalidOperationException("illegal bal type");
                }
            }

            internal static BAL CreateInstanceOfType(BalType balType, IEnumerable<StackSymbolSequenceSet<S>> bottoms)
            {
                switch (balType)
                {
                    case BalType.BAL_L: return new BAL_L(bottoms);
                    case BalType.BAL_R: return new BAL_R(bottoms);
                    default: throw new InvalidOperationException("illegal bal type");
                }
            }

            internal abstract void SetCorrespondingSide(Node<A, S> node, StackSymbolSequenceSet<S> modifiedSite, StackSymbolSequenceSet<S> otherSide);

            internal enum BalType { BAL_L, BAL_R };
        }

        internal class BAL_L : BAL
        {
            internal BAL_L(IEnumerable<StackSymbolSequenceSet<S>> bottoms) : base(bottoms)
            {
            }

            internal static Func<Node<A, S>, StackSymbolSequenceSet<S>> SideSelector => (node) => node.LeftHand;

            internal override BalType GetBALType()
            {
                return BalType.BAL_L;
            }

            internal override bool HasBALType(BalType balType)
            {
                return balType == BalType.BAL_L;
            }

            internal override void SetCorrespondingSide(Node<A, S> node, StackSymbolSequenceSet<S> modifiedSite, StackSymbolSequenceSet<S> otherSide)
            {
                node.LeftHand = modifiedSite;
                node.RightHand = otherSide;
            }
        }

        internal class BAL_R : BAL
        {
            internal BAL_R(IEnumerable<StackSymbolSequenceSet<S>> bottoms) : base(bottoms)
            {
            }

            internal static Func<Node<A, S>, StackSymbolSequenceSet<S>> SideSelector => (node) => node.RightHand;

            internal override BalType GetBALType()
            {
                return BalType.BAL_R;
            }

            internal override bool HasBALType(BalType balType)
            {
                return balType == BalType.BAL_R;
            }

            internal override void SetCorrespondingSide(Node<A, S> node, StackSymbolSequenceSet<S> modifiedSite, StackSymbolSequenceSet<S> otherSide)
            {
                node.LeftHand = otherSide;
                node.RightHand = modifiedSite;
            }
        }

        internal class UNF : Operation<A, S>
        {
            internal A Symbol { get; }

            internal UNF(A symbol)
            {
                Symbol = symbol;
            }

            internal override bool HasBALType(BAL.BalType balType)
            {
                return false;
            }
        }
    }
}
