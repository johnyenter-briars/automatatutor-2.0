using AutomataPDL.PDA.SDA;
using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.DPDA.DPDAEquivalence
{
    class Node<A, S> where S : IEquatable<S> where A : IEquatable<A>
    {
        internal StackSymbolSequenceSet<S> LeftHand { get; set; }
        internal StackSymbolSequenceSet<S> RightHand { get; set; }

        internal Node<A, S> Parent { get; }
        internal IEnumerable<Node<A, S>> Children { get; private set; }

        internal Operation<A, S> OperationToHere { get; }

        private DeterminisedSDA<A, S> sda;
        private IEnumerable<A> symbols;

        public Node(StackSymbolSequenceSet<S> leftHand, StackSymbolSequenceSet<S> rightHand, Operation<A, S> operationToHere, Node<A, S> parent, DeterminisedSDA<A, S> sda, IEnumerable<A> symbols)
            : this(operationToHere, parent, sda, symbols)
        {
            LeftHand = leftHand;
            RightHand = rightHand;
        }

        public bool IsSuccessfull()
        {
            if (RightHand.Equals(LeftHand))
            {
                return true;
            }
            return ObeysExtensionTheorem();
        }

        private bool ObeysExtensionTheorem()
        {
            var branch = GetWholeBranch();

            //TODO: implement as described in the paper at page 28

            return false; //FIXME
        }

        public bool IsUnSuccessfull()
        {
            return LeftHand.IsEmptySet() && !RightHand.IsEmptySet() || !LeftHand.IsEmptySet() && RightHand.IsEmptySet();
        }

        private Node(Operation<A, S> operationToHere, Node<A, S> parent, DeterminisedSDA<A, S> sda, IEnumerable<A> symbols)
        {
            OperationToHere = operationToHere;
            Parent = parent;
            this.sda = sda;
            this.symbols = symbols;
        }

        private bool HasAsPredecessor(Node<A, S> other)
        {
            var currentNode = this;

            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
                if (currentNode == other)
                {
                    return true;
                }
            }
            return false;
        }

        //TODO: check if already finished here (either correct, then do not apply this node anymore or incorrect, then stop)
        public void NextStep()
        {
            var balSetting = CheckIfBALIsPossible(Operation<A, S>.BAL.BalType.BAL_L);
            if (balSetting != null)
            {
                ApplyBAL(Operation<A, S>.BAL.BalType.BAL_L, balSetting);
            }
            else
            {
                balSetting = CheckIfBALIsPossible(Operation<A, S>.BAL.BalType.BAL_R);
                if (balSetting != null)
                {
                    ApplyBAL(Operation<A, S>.BAL.BalType.BAL_R, balSetting);
                }
                else
                {
                    Children = symbols
                        .Select(k => new Node<A, S>(
                        sda.ApplySymbolToStackSymbolSequenceSet(LeftHand, k),
                        sda.ApplySymbolToStackSymbolSequenceSet(RightHand, k),
                        new Operation<A, S>.UNF(k),
                        this,
                        sda,
                        symbols));
                }
            }
        }

        private void ApplyBAL(Operation<A, S>.BAL.BalType balType, BALSetting<A, S> bALSetting)
        {
            var oppositeSideSelector = Operation<A, S>.BAL.GetOppositeSideSelector(balType);
            var F = oppositeSideSelector(bALSetting.Premise);
            var bottoms = bALSetting.PremiseHeads.Select(x => sda.ApplyWordToStackSymbolSequenceSet(F, sda.ShortestWordsOfStackSymbols[x])).ToList();

            var op = Operation<A, S>.BAL.CreateInstanceOfType(balType, bottoms);
            var child = new Node<A, S>(op, this, sda, symbols);
            Children = new List<Node<A, S>>() { child };

            var modifiedSideUnflattend = bALSetting.OwnHeads.Zip(bottoms, (head, bottom) => head.Multiply(bottom)).ToList();
            var modifiedSite = StackSymbolSequenceSet<S>.Flatten(modifiedSideUnflattend);
            op.SetCorrespondingSide(child, modifiedSite, oppositeSideSelector(this));
        }

        private BALSetting<A, S> CheckIfBALIsPossible(Operation<A, S>.BAL.BalType balType)
        {
            var currentNode = this;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
                var balSetting = IsBALPermitted(balType, currentNode);
                if (balSetting != null)
                {
                    return balSetting;
                }
            }
            return null;
        }

        /// <summary>
        /// checks whether the defined BAL-operation can be applied; important: e.g. for BAL_R is NOT checked whether BAL_L is also permitted!
        /// </summary>
        /// <param name="premise"></param>
        /// <returns>the list of the own heads and the heads of the premise</returns>
        private BALSetting<A, S> IsBALPermitted(Operation<A, S>.BAL.BalType balType, Node<A, S> premise)
        {
            //TODO: maybe hand this over as parameter for performance
            var nodeWithLastBAL = FindLastBAL();

            if (nodeWithLastBAL == null || nodeWithLastBAL.OperationToHere.HasBALType(balType))
            {
                return CheckBALSideCondition(Operation<A, S>.BAL.GetSideSelector(balType), premise);
            }

            Assertion.Assert(!(nodeWithLastBAL.OperationToHere is Operation<A, S>.UNF), "the last BAL operation has to be a BAL");

            //Hint: the premise has to be after the last application of BAL (in particular it cannot be the same node)
            var premiseIsAfterLastBAL = premise.HasAsPredecessor(nodeWithLastBAL);
            if (premiseIsAfterLastBAL)
            {
                var partitialBranch = premise.GetBranchToHereFromExlusively(nodeWithLastBAL);
                var balOperation = (Operation<A, S>.BAL)nodeWithLastBAL.OperationToHere;
                var bottoms = balOperation.Bottoms;
                var sideSelector = Operation<A, S>.BAL.GetSideSelector(balOperation.GetBALType());

                var bottomWasExposed = partitialBranch.Any(n => bottoms.Any(bottom => sideSelector(n).Equals(bottom)));

                if (bottomWasExposed)
                {
                    return CheckBALSideCondition(Operation<A, S>.BAL.GetSideSelector(balType), premise);
                }
            }

            return null;
        }

        private Node<A, S> FindLastBAL()
        {
            var currentNode = this;
            while (currentNode.Parent != null)
            {
                if (currentNode.OperationToHere is Operation<A, S>.BAL)
                {
                    return currentNode;
                }
                currentNode = currentNode.Parent;
            }
            return null;
        }

        private BALSetting<A, S> CheckBALSideCondition(Func<Node<A, S>, StackSymbolSequenceSet<S>> sideSelector, Node<A, S> premise)
        {
            Assertion.Assert(SelfAndPredecessors().Contains(this), "the premise has to be a predecessor of the node");

            var branch = GetBranchToHereFromExlusively(premise);

            if (!branch.All(n => n.OperationToHere is Operation<A, S>.UNF))
            {
                return null;
            }

            var branchWord = branch.Select(n => ((Operation<A, S>.UNF)n.OperationToHere).Symbol);

            var premiseSide = sideSelector(premise);
            var ownSide = sideSelector(this);

            var premiseHeadTail = premiseSide.Get1HeadTailForm();

            var expOwnHeads = premiseHeadTail.Head.StackSequenceSet.Select(stackSymbol => sda.ApplyWordToStackSymbolSequence(stackSymbol, branchWord)).ToList();

            if (!CheckBALSideCondition1(expOwnHeads, premiseHeadTail.Tail))
            {
                return null;
            }

            var expOwnExprUnflattend = expOwnHeads.Zip(premiseHeadTail.Tail.StackSequenceSet, (ownHead, tail) => ownHead.Multiply(tail)).ToList();
            var epxOwnExpr = StackSymbolSequenceSet<S>.Flatten(expOwnExprUnflattend);

            //Is it correct, that it is unnecessary to verify that the expression is in valid head/tail-form (that means that the 
            //heads only are admissible)? I think it is, as the 1-k-form of the premise is valid head/tail-form and according to Chapter 4 Proposition 4
            //the derived expression should also be in valid head/tail-form
            if (!ownSide.Equals(epxOwnExpr))
            {
                return null;
            }

            if (CheckBALSideCondition2(expOwnHeads, premiseHeadTail.Head, branch))
            {
                var premiseHeads = premiseHeadTail.Head.StackSequenceSet.Select(x =>
                {
                    Assertion.Assert(x.StackSequence.Count() == 1, "x should contain exactly one stack symbol");
                    return x.StackSequence.First();
                }).ToList();

                return new BALSetting<A, S>(premise, expOwnHeads, premiseHeads);
            }

            return null;
        }

        private bool CheckBALSideCondition1(List<StackSymbolSequenceSet<S>> ownHeads, StackSymbolSequenceSet<S> premiseTail)
        {
            var ownHeadsAreNotEmpty = ownHeads.All(h => !h.IsEpsilonSet());
            if (!ownHeadsAreNotEmpty)
            {
                return false;
            }

            var atLeastOneTailIsNotEmpty = premiseTail.StackSequenceSet.Any(s => !s.IsEpsilon());
            return atLeastOneTailIsNotEmpty;
        }

        private bool CheckBALSideCondition2(List<StackSymbolSequenceSet<S>> ownHeads, StackSymbolSequenceSet<S> premiseHeads, IEnumerable<Node<A, S>> branch)
        {
            var premiseHeadsAndOwnHeads = premiseHeads.StackSequenceSet.Zip(ownHeads, (stackSymbol, ownHead) => new { stackSymbol, ownHead });
            var premiseHeadsWhereOwnHeadNotEmptySet = premiseHeadsAndOwnHeads.Where(t => !t.ownHead.IsEmptySet()).Select(t => t.stackSymbol).ToList();

            var maxPremiseHeadNorm = premiseHeadsWhereOwnHeadNotEmptySet.Max(x =>
            {
                Assertion.Assert(x.StackSequence.Count() == 1, "the premise heads have to have length one");
                return sda.ShortestWordsOfStackSymbols[x.StackSequence.First()].Count();
            });

            return branch.Count() == maxPremiseHeadNorm;
        }

        internal IEnumerable<Node<A, S>> SelfAndPredecessors()
        {
            var res = new List<Node<A, S>>();
            var currentNode = this;
            while (currentNode.Parent != null)
            {
                res.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            return res;
        }

        internal IEnumerable<Node<A, S>> GetBranchToHereFromExlusively(Node<A, S> premise)
        {
            var res = new List<Node<A, S>>();
            var currentNode = this;
            while (currentNode != premise)
            {
                res.Insert(0, currentNode);
                currentNode = currentNode.Parent;
            }
            return res;
        }

        private IEnumerable<Node<A, S>> GetWholeBranch()
        {
            var res = new List<Node<A, S>>();
            var currentNode = this;
            while (currentNode.Parent != null)
            {
                res.Insert(0, currentNode);
                currentNode = currentNode.Parent;
            }
            res.Insert(0, currentNode);
            return res;
        }
    }
}
