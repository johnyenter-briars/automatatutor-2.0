using AutomataPDL.CFG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;
using System.Threading;

namespace AutomataPDL.PDA.PDARunner
{
    static class PDAToCFGConverter<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal static ContextFreeGrammar ToCFG(PDA<A, S> pda)
        {
            return ToCFG(pda, new CancellationTokenSource().Token);
        }

        internal static ContextFreeGrammar ToCFG(PDA<A, S> pda, CancellationToken token)
        {
            Assertion.Assert(pda.AcceptanceCondition.IsEmptyStack(), "only a pda with acceptance condition 'empty stack' can be converted to a cfg");

            var start = new Nonterminal("S");
            var nonTerminals = new Dictionary<string, Nonterminal>();
            var terminals = new Dictionary<A, Exprinal<A>>();

            var nonTerminalsList = pda.States.SelectMany(s1 =>
                pda.AllStackSymbols.SelectMany(stackSymbol =>
                    pda.States.Select(s2 =>
                    {
                        token.ThrowIfCancellationRequested();
                        var key = GetNonTerminalId(s1.Value, stackSymbol, s2.Value);
                        return new { key, nonterminal = new Nonterminal(key) };
                    })));
            foreach (var n in nonTerminalsList)
            {
                token.ThrowIfCancellationRequested();
                nonTerminals.Add(n.key, n.nonterminal);
            }

            var terminalList = pda.States.SelectMany(state => state.Value.Transitions.Select(t => t.SymbolIn)).Where(a => !a.IsEmpty()).ToList();
            foreach(var t in terminalList)
            {
                if (!terminals.ContainsKey(t.GetSymbol()))
                {
                    terminals.Add(t.GetSymbol(), new Exprinal<A>(t.GetSymbol(), t.GetSymbol().ToString()));
                }
            }

            var productions = GetInitialProductions(pda, nonTerminals, start, token);
            AddProductionsFromTransitions(pda, nonTerminals, terminals, productions, token);

            return new ContextFreeGrammar(start, productions);
        }

        private static void AddProductionsFromTransitions(PDA<A, S> pda, Dictionary<string, Nonterminal> nonTerminals, Dictionary<A, Exprinal<A>> terminals, List<Production> productions, CancellationToken token)
        {
            foreach (var state in pda.States)
            {
                foreach (var transition in state.Value.Transitions)
                {
                    token.ThrowIfCancellationRequested();
                    AddProductionsOfTransition(state.Value, transition, pda, nonTerminals, terminals, productions, token);
                }
            }
        }

        private static void AddProductionsOfTransition(State<A, S> state, Transition<A, S> transition, PDA<A, S> pda, Dictionary<string, Nonterminal> nonTerminals, Dictionary<A, Exprinal<A>> terminals, List<Production> productions, CancellationToken token)
        {
            var num = transition.StackSymbolsWritten.Count();
            var allPossibleStates = GetAllPossibleStateListsWithLength(num, pda, token).ToList();
            if (allPossibleStates.Count() > 0)
            {
                foreach (var states in allPossibleStates)
                {
                    token.ThrowIfCancellationRequested();
                    List<GrammarSymbol> targets = states.Select((s, i) =>
                    {
                        State<A, S> firstState;
                        if (i == 0)
                        {
                            firstState = transition.Target;
                        }
                        else
                        {
                            firstState = states.ElementAt(i - 1);
                        }

                        return GetNonTerminal(nonTerminals, firstState, transition.StackSymbolsWritten.ElementAt(i), s);
                    }).ToList<GrammarSymbol>();

                    if (!transition.SymbolIn.IsEmpty())
                    {
                        var symbol = transition.SymbolIn.GetSymbol();
                        targets.Insert(0, terminals[symbol]);
                    }

                    productions.Add(new Production(GetNonTerminal(nonTerminals, state, transition.StackSymbolIn, states.Last()), targets.ToArray()));
                }
            }
            else
            {
                if (transition.SymbolIn.IsEmpty())
                {
                    productions.Add(new Production(GetNonTerminal(nonTerminals, state, transition.StackSymbolIn, transition.Target)));
                }
                else
                {
                    var symbol = transition.SymbolIn.GetSymbol();
                    var target = terminals[symbol];
                    productions.Add(new Production(GetNonTerminal(nonTerminals, state, transition.StackSymbolIn, transition.Target), target));
                }
            }
        }

        private static IEnumerable<List<State<A, S>>> GetAllPossibleStateListsWithLength(int length, PDA<A, S> pda, CancellationToken token)
        {
            IEnumerable<List<State<A, S>>> allPossibleStates = new List<List<State<A, S>>>();
            if (length > 0)
            {
                allPossibleStates = allPossibleStates.Concat(new[] { new List<State<A, S>>() });
            }
            for (int i = 0; i < length; i++)
            {
                token.ThrowIfCancellationRequested();
                allPossibleStates = allPossibleStates.SelectMany(states => {
                    token.ThrowIfCancellationRequested();
                    return pda.States.Select(s => states.Concat(new[] { s.Value }).ToList()).ToList();
                });
            }
            return allPossibleStates;
        }

        private static List<Production> GetInitialProductions(PDA<A, S> pda, Dictionary<string, Nonterminal> nonTerminals, Nonterminal start, CancellationToken token)
        {
            var productions = new List<Production>();
            foreach (var state in pda.States)
            {
                token.ThrowIfCancellationRequested();
                productions.Add(new Production(start, GetNonTerminal(nonTerminals, pda.InitialState, pda.FirstStackSymbol, state.Value)));
            }
            return productions;
        }

        static readonly char separator = ';';
        public static string GetNonTerminalId(State<A, S> state1, S stackSymbol, State<A, S> state2)
        {
            return state1.Id.ToString() + separator + stackSymbol.ToString() + separator + state2.Id.ToString();
        }

        public static Tuple<int, S, int> SplitNonTerminalId(string nonTerminalId)
        {
            var firstIndexOfSep = nonTerminalId.IndexOf(separator);
            var lastIndexOfSep = nonTerminalId.LastIndexOf(separator);
            var idState1 = int.Parse(nonTerminalId.Substring(0, firstIndexOfSep));
            var stackSymbols = (S) Convert.ChangeType(nonTerminalId.Substring(firstIndexOfSep + 1, lastIndexOfSep - firstIndexOfSep - 1), typeof(S));
            var idState2 = int.Parse(nonTerminalId.Substring(lastIndexOfSep + 1));
            return new Tuple<int, S, int>(idState1, stackSymbols, idState2);
        }

        private static Nonterminal GetNonTerminal(Dictionary<string, Nonterminal> nonTerminals, State<A, S> state1, S stackSymbol, State<A, S> state2)
        {
            return nonTerminals[GetNonTerminalId(state1, stackSymbol, state2)];
        }
    }
}
