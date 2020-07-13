using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutomataPDL.PDA.PDARunner;
using AutomataPDL.PDA.Simulation;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDA
{
    /// <summary>
    /// represents a whole PDA
    /// </summary>
    /// <typeparam name="A">type of the alphabet letters</typeparam>
    /// <typeparam name="S">type of the stack alphabet symbols</typeparam>
    public class PDA<A, S> : IEquatable<PDA<A, S>> where S : IEquatable<S> where A : IEquatable<A>
    {
        internal const int initialStateId = 0;

        public AcceptanceCondition AcceptanceCondition { get; }
        internal Dictionary<int, State<A, S>> States { get; }
        internal State<A, S> InitialState { get; }
        internal bool Deterministic { get; }
        internal S FirstStackSymbol { get; }
        internal IEnumerable<S> AllStackSymbols { private set; get; }

        /// <summary>
        /// defines whether the PDA is in normal form: that means, that for every transition the maximum number of pushed stack-symbols is 2 
        /// and no Epsilon-Transition does push any stack-symbols
        /// </summary>
        internal bool NormalForm { get; private set; } = false;

        /// <summary>
        /// this is used for converting a PDA from final state to empty stack
        /// </summary>
        internal S FurtherStackSymbol { get; }

        IPDARunner<A, S> pdaRunner;

        delegate string getStateErrorMessage(int id);

        readonly getStateErrorMessage getAlreadyExistsError = id => "there already exists a state with id " + id;
        readonly getStateErrorMessage getNotExistsError = id => "there does not exist a node with id " + id;
        const string InvalidStackSymbolError = "one of the stack symbols is not in the stack symbol list of this pda";

        public PDA(AcceptanceCondition acceptanceCondition, bool deterministic, S firstStackSymbol, bool initialStateIsFinal, IEnumerable<S> allStackSymbols, S furtherStackSymbol)
            : this(acceptanceCondition, deterministic, firstStackSymbol, initialStateIsFinal, allStackSymbols)
        {
            Assertion.Assert(!allStackSymbols.Contains(furtherStackSymbol), "the further stack symbol must not be in the list of all stack symbols");

            FurtherStackSymbol = furtherStackSymbol;
        }

        public PDA(AcceptanceCondition acceptanceCondition, bool deterministic, S firstStackSymbol, bool initialStateIsFinal, IEnumerable<S> allStackSymbols)
        {
            Assertion.Assert(allStackSymbols.Distinct().Count() == allStackSymbols.Count(), "at least one stack symbol occurs twice in the stack symbol list");

            AcceptanceCondition = acceptanceCondition;
            Deterministic = deterministic;
            FirstStackSymbol = firstStackSymbol;
            InitialState = new State<A, S>(initialStateId, initialStateIsFinal);
            AllStackSymbols = allStackSymbols.ToList();
            States = new Dictionary<int, State<A, S>>
            {
                { InitialState.Id, InitialState }
            };
        }

        public void AddStackSymbol(S stackSymbol)
        {
            Assertion.Assert(!AllStackSymbols.Any(s => s.Equals(stackSymbol)), "this stack symbol already exists in the stack symbol list of the pda");
            AllStackSymbols = AllStackSymbols.Concat(new List<S>() { stackSymbol }).ToList();
        }

        public void HasNormalForm()
        {
            Assertion.Assert(CheckIfHasNormalForm(), "the pda is not in normal form");
            NormalForm = true;
        }

        private bool CheckIfHasNormalForm()
        {
            return States.All(s => s.Value.Transitions.All(t => Transition<A, S>.HasNormalForm(t.SymbolIn, t.StackSymbolsWritten)));
        }

        /// <summary>
        /// must be called before invoking <see cref="PDA{A, S}.AcceptsWord(IEnumerable{A})"/> or 
        /// <see cref="PDA{A, S}.AcceptsWordOrInconsistent(IEnumerable{A})"/>
        /// </summary>
        public void CreateRunner()
        {
            CreateRunner(new CancellationTokenSource().Token);
        }

        public void CreateRunner(CancellationToken token)
        {
            if (Deterministic)
            {
                pdaRunner = new DPDARunner<A, S>(this, token);
            }
            else
            {
                pdaRunner = new PDARunnerWithCFG<A, S>(this, token);
            }
        }

        public static PDA<char, char> FromXml(XElement xmlPDA)
        {
            return PDAXmlParser.ParsePDAFromXmlPDA(xmlPDA);
        }

        public void AddState(int id, bool isFinal)
        {
            Assertion.Assert(!States.ContainsKey(id), getAlreadyExistsError(id));
            States.Add(id, new State<A, S>(id, isFinal));
        }

        internal void AddTransition(int startId, int targetId, Symbol<A> symbolIn, S stackSymbolIn, S[] stackSymbolsWritten)
        {
            Assertion.Assert(States.ContainsKey(startId), getNotExistsError(startId));
            Assertion.Assert(States.ContainsKey(targetId), getNotExistsError(targetId));
            Assertion.Assert(AllStackSymbols.Any(s => s.Equals(stackSymbolIn)), InvalidStackSymbolError);
            Assertion.Assert(stackSymbolsWritten.All(s => AllStackSymbols.Any(t => t.Equals(s))), InvalidStackSymbolError);
            Assertion.Assert(States[startId].Transitions.Where(t => t.Target.Id == targetId
            && t.SymbolIn.Equals(symbolIn) && t.StackSymbolIn.Equals(stackSymbolIn)
            && stackSymbolsWritten.SequenceEqual(t.StackSymbolsWritten))
            .Count() == 0, "this transitions already exists");

            Assertion.Assert(!Deterministic || IsStillDeterministic(startId, symbolIn, stackSymbolIn), "the new transition violates the determinism-property");

            Assertion.Assert(!NormalForm || Transition<A, S>.HasNormalForm(symbolIn, stackSymbolsWritten), "this pda has normal form, but the new transition violates this property");

            States[startId].AddTransition(States[targetId], symbolIn, stackSymbolIn, stackSymbolsWritten);
        }

        private bool IsStillDeterministic(int startId, Symbol<A> symbolIn, S stackSymbolIn)
        {
            return States[startId].Transitions.Where(t => t.StackSymbolIn.Equals(stackSymbolIn)
            && (t.SymbolIn.Equals(symbolIn) || t.SymbolIn.IsEmpty() || symbolIn.IsEmpty())).Count() == 0;
        }

        internal void RemoveTransition(Transition<A, S> transition)
        {
            var state = States.Where(s => s.Value.Transitions.Contains(transition));
            Assertion.Assert(state.Count() <= 1, "illegal state: the transitionto remove occurs more than once");
            state.First().Value.Transitions.Remove(transition);
        }

        public TransitionBuilder<A, S>.FromBuilder AddTransition()
        {
            return TransitionBuilder<A, S>.BuildTrasition(this);
        }

        public AcceptanceResult<S> AcceptsWord(IEnumerable<A> word)
        {
            Assertion.Assert(pdaRunner != null, "when creating a pda, you have to call the method 'createRunner' first in order to signalize, that you have finished adding all states and transitions");
            var res = pdaRunner.IsWordAccepted(word);

            Assertion.Assert(!res.IsInconsistent(), () => new InconsistentPDAException(
                string.Format("the acceptance conditions of the pda are final-state and empty-stack, but the acceptance of \"{0}\" is inconsistent: for final-state is {1}, for empty stack is {2}", 
                word, res.AcceptedByAcceptedCondition[AcceptanceCondition.FinalState.GetId()], !res.AcceptedByAcceptedCondition[AcceptanceCondition.FinalState.GetId()])));
            return res;
        }

        /// <summary>
        /// like AcceptsWord, but does not throw an exception when the acceptance condition is inconsistent
        /// </summary>
        /// <param name="word"></param>
        /// <returns>acceptance result of the word</returns>
        public AcceptanceResult<S> AcceptsWordOrInconsistent(IEnumerable<A> word)
        {
            Assertion.Assert(pdaRunner != null, "when creating a pda, you have to set the 'createRunner'-parameter to true to be able to solve the word-problem");
            return pdaRunner.IsWordAccepted(word);
        }

        public bool Equals(PDA<A, S> other)
        {
            var part1 = AcceptanceCondition.Equals(other.AcceptanceCondition);
            var part2 = Deterministic == other.Deterministic;
            var part3 = InitialState.Equals(other.InitialState);
            var part4 = FirstStackSymbol.Equals(other.FirstStackSymbol);
            var part5 = AllStackSymbols.OrderBy(s => s.ToString()).SequenceEqual(other.AllStackSymbols.OrderBy(s => s.ToString()));
            var part6 = States.Values.OrderBy(s => s.Id).SequenceEqual(other.States.Values.OrderBy(s => s.Id));
            return part1 && part2 && part3 && part4 && part5 && part6;
        }

        public PDA<A, S> Clone()
        {
            var res = new PDA<A, S>(AcceptanceCondition, true, FirstStackSymbol, InitialState.Final, AllStackSymbols.ToList());
            foreach (var state in States.Where(s => s.Key != initialStateId).ToList())
            {
                res.AddState(state.Key, state.Value.Final);
            }
            foreach(var state in States)
            {
                foreach(var t in state.Value.Transitions)
                {
                    res.AddTransition().From(t.Origin.Id).To(t.Target.Id).Read(t.SymbolIn).Pop(t.StackSymbolIn).Push(t.StackSymbolsWritten);
                }
            }
            return res;
        }
    }
}
