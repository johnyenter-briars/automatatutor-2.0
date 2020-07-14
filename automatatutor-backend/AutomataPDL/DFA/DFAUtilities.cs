using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Microsoft.Automata;
using Microsoft.Z3;

using System.Diagnostics;

using System.Text.RegularExpressions;
using AutomataPDL.Automata;

namespace AutomataPDL
{
    public static class DFAUtilities
    {
        public static Pair<HashSet<char>, Automaton<BDD>> parseDFAFromString(string str, CharSetSolver solver)
        {
            var lines = Regex.Split(str, "\r\n|\r|\n");
            HashSet<char> al = new HashSet<char>();

            var line = lines[0];
            var tokens = line.Split(new char[] { ' ' });
            for (int i = 1; i < tokens.Length; i++)
                al.Add(tokens[i].ToCharArray()[0]);

            var finalStates = new List<int>();
            line = lines[2];
            tokens = line.Split(new char[] { ' ' });
            for (int i = 2; i < tokens.Length; i++)
                finalStates.Add(Convert.ToInt32(tokens[i]));

            var moves = new List<Move<BDD>>();
            for (int i = 3; i < lines.Length; i++)
            {
                tokens = lines[i].Split(new char[] { ',' });
                if (tokens.Length > 1)
                    moves.Add(new Move<BDD>(Convert.ToInt32(tokens[0]), Convert.ToInt32(tokens[1]), solver.MkCharConstraint(false, tokens[2].ToCharArray()[0])));
            }

            return new Pair<HashSet<char>, Automaton<BDD>>(al, Automaton<BDD>.Create(0, finalStates, moves));
        }

        public static Pair<HashSet<char>, Automaton<BDD>> parseDFAFromXML(XElement Automaton1, CharSetSolver solver)
        {
            HashSet<char> al = new HashSet<char>();
            //XElement Automaton = XElement.Parse(xmlString);

            //All DFAs in problem set on automata tutor are over a,b
            

            var moves = new List<Move<BDD>>();
            var finalStates = new List<int>();
            int start = 0;

            XElement Automaton = XElement.Parse(RemoveAllNamespaces(Automaton1.ToString()));

            XElement trans = Automaton.Element("transitionSet");

            foreach (XElement child in trans.Elements())
            {
                if (child.Name == "transition")
                {
                    moves.Add(new Move<BDD>(Convert.ToInt32(child.Element("from").Value), Convert.ToInt32(child.Element("to").Value),
                        solver.MkCharConstraint(false, Convert.ToChar(child.Element("read").Value))));
                    al.Add(Convert.ToChar(child.Element("read").Value));
                }
            }

            XElement acc = Automaton.Element("acceptingSet");
            foreach (XElement child in acc.Elements())
            {
                if (child.Name == "state")
                {
                    finalStates.Add((int)child.Attribute("sid"));
                }
            }

            XElement states = Automaton.Element("initState");
            foreach (XElement child in states.Elements())
            {
                if (child.Name == "state")
                {
                    start = (int)child.Attribute("sid");
                }
            }

            return new Pair<HashSet<char>, Automaton<BDD>>(al, Automaton<BDD>.Create(start, finalStates, moves));

        }

        public static List<Pair<HashSet<char>, Automaton<BDD>>> parseDFAListFromXML(XElement automataList, CharSetSolver solver)
        {
            var outList = new List<Pair<HashSet<char>, Automaton<BDD>>>();
            foreach(XElement Automaton in automataList.Elements()) {
                outList.Add(parseDFAFromXML(Automaton.Elements().First(), solver));
            }

            return outList;
        }

        // Automaton should be rooted at automaton tag
        public static Pair<HashSet<char>, Automaton<BDD>> parseBlockFromXML(XElement automaton, CharSetSolver solver)
        {
            XElement Automaton = RemoveJustNamespaces(automaton);
            XElement xAlphabet = Automaton.Element("alphabet");
            return parseBlockFromXML(Automaton.Element("block"), xAlphabet, solver);
        }

        // Does not support epsilon transitions
        public static NFA<char, string> toNFA(Automaton<BDD> automaton, HashSet<char> alphabet, CharSetSolver solver)
        {
            var nodes = automaton.States;
            HashSet<State<string>> statesNFA = new HashSet<State<string>>();
            Dictionary<int, State<string>> statesMap = new Dictionary<int, State<string>>();
            foreach(int s in nodes)
            {
                var state = new State<string>(s.ToString());
                statesMap.Add(s, state);
                statesNFA.Add(state);
            }
            Dictionary<TwoTuple<State<string>, char>, HashSet<State<string>>> transitionsNFA = new Dictionary<TwoTuple<State<string>, char>, HashSet<State<string>>>();
            foreach(var s in nodes)
                foreach(var c in alphabet)
                {
                    var follows = GetNextNfaStates(s, c, automaton, solver);
                    if (follows.Count > 0)
                        transitionsNFA.Add(new TwoTuple<State<string>, char>(statesMap[s], c), follows);
                }
            var finalsNFA = new HashSet<State<string>>();
            var finals = automaton.GetFinalStates();
            foreach (int s in finals)
                finalsNFA.Add(statesMap[s]);
            var initialNFA = new HashSet<State<string>>();
            initialNFA.Add(statesMap[automaton.InitialState]);

            return new NFA<char, string>(statesNFA, alphabet, transitionsNFA, initialNFA, finalsNFA);
        }

        // Automaton should be rooted at block tag, not automaton tag
        public static Pair<HashSet<char>, Automaton<BDD>> parseBlockFromXML(XElement automaton, XElement xAlphabet, CharSetSolver solver)
        {
            HashSet<char> alphabet = new HashSet<char>();
            Dictionary<int, Automaton<BDD>> subAutomaton = new Dictionary<int, Automaton<BDD>>();

            var moves = new List<Move<BDD>>();
            var finalStates = new List<int>();
            int start = 0;

            foreach(var child in xAlphabet.Elements("symbol"))
            {
                char element = Convert.ToChar(child.Value.Trim());
                alphabet.Add(element);
            }
            XElement Automaton = RemoveJustNamespaces(automaton);
            XElement states = Automaton.Element("stateSet");
            foreach(XElement child in states.Elements())
            {
                var x = states.Elements();
                if(child.Name == "block")
                {
                    string regex = child.Attribute("regex").Value.Trim();
                    XElement xRegex = XElement.Parse(string.Format("<div>{0}</div>", regex));

                    Pair<HashSet<char>, Automaton<BDD >> dfaPair = DFAUtilities.parseRegexFromXML(xRegex, xAlphabet, solver);
                    int id = Convert.ToInt32(child.Attribute("sid").Value);

                    var edges = dfaPair.Second.GetMoves();
                    var uniqueEdges = new List<Move<BDD>>();
                    foreach (Move<BDD> edge in edges)
                    {
                        int source = edge.SourceState + id * 100;
                        int target = edge.TargetState + id * 100;
                        uniqueEdges.Add(new Move<BDD>(source, target, edge.Label));
                    }
                    int initial = dfaPair.Second.InitialState + id * 100;
                    var finalNodes = dfaPair.Second.GetFinalStates();
                    var uniqueFinals = new List<int>();
                    foreach (int state in finalNodes)
                    {
                        uniqueFinals.Add(state + id * 100);
                    }
                    var aut = Automaton<BDD>.Create(initial, uniqueFinals, uniqueEdges);
                    subAutomaton[id] = aut;
                }
            }

            // Adding edges between components
            XElement trans = Automaton.Element("transitionSet");
            foreach (XElement child in trans.Elements())
            {
                if (child.Name == "transition")
                {
                    var letters = child.Element("label").Value.Trim().Split(' ');
                    
                    // Initial source and target. May be block nodes
                    int source = Convert.ToInt32(child.Element("from").Value);
                    int target = Convert.ToInt32(child.Element("to").Value);
                    IEnumerable<int> sourceList = new List<int> {source};
                    // In case source is a block node, we need to connect all the final nodes of the source to the target node
                    if (subAutomaton.ContainsKey(source))
                        sourceList = subAutomaton[source].GetFinalStates();
                    // In case target is a block node, we just reassign taget to be the initial node of the target block
                    if (subAutomaton.ContainsKey(target))
                        target = subAutomaton[target].InitialState;
                    foreach (var l in letters)
                    {
                        char element = Convert.ToChar(l.Trim());
                        if (element != 'ε' && element != '?')
                            foreach (int s in sourceList)
                                moves.Add(new Move<BDD>(s, target, solver.MkCharConstraint(false, element)));
                        else
                            foreach (int s in sourceList)
                                moves.Add(Move<BDD>.Epsilon(s, target));
                    }
                }
            }
            // Adding edges in subcomponents
            foreach (KeyValuePair<int, Automaton<BDD>> entry in subAutomaton)
                foreach (Move<BDD> move in entry.Value.GetMoves())
                    moves.Add(move);
            
            // Adding final states of big Automaton
            XElement acc = Automaton.Element("acceptingSet");
            foreach (XElement child in acc.Elements())
            {
                if (child.Name == "state")
                {
                    finalStates.Add((int)child.Attribute("sid"));
                }
                else
                {
                    int id = (int)child.Attribute("sid");
                    foreach (int state in subAutomaton[id].GetFinalStates())
                        finalStates.Add(state);
                }
            }

            XElement initStates = Automaton.Element("initialState");
            foreach (XElement child in initStates.Elements())
            {
                if (child.Name == "state")
                {
                    start = (int)child.Attribute("sid");
                }
            }

            return new Pair<HashSet<char>, Automaton<BDD>>(alphabet, Automaton<BDD>.Create(start, finalStates, moves));
        }

        public static Pair<HashSet<char>, Automaton<BDD>> parseNFAFromXML(XElement Automaton1, CharSetSolver solver)
        {
            HashSet<char> al = new HashSet<char>();

            var moves = new List<Move<BDD>>();
            var finalStates = new List<int>();
            int start = 0;

            XElement Automaton = XElement.Parse(RemoveAllNamespaces(Automaton1.ToString()));
            XElement xmlAlphabet = Automaton.Element("alphabet");
            foreach (XElement child in xmlAlphabet.Elements())
            {
                char element = Convert.ToChar(child.Value);
                if (element != 'ε' && element != '?')
                    al.Add(element);
            }


            XElement trans = Automaton.Element("transitionSet");

            foreach (XElement child in trans.Elements())
            {
                if (child.Name == "transition")
                {
                    char element = Convert.ToChar(child.Element("read").Value);
                    if (element != 'ε' && element != '?')
                        moves.Add(new Move<BDD>(Convert.ToInt32(child.Element("from").Value), Convert.ToInt32(child.Element("to").Value),
                            solver.MkCharConstraint(false, element)));
                    else
                        moves.Add(Move<BDD>.Epsilon(Convert.ToInt32(child.Element("from").Value), Convert.ToInt32(child.Element("to").Value)));

                    
                }
            }

            XElement acc = Automaton.Element("acceptingSet");
            foreach (XElement child in acc.Elements())
            {
                if (child.Name == "state")
                {
                    finalStates.Add((int)child.Attribute("sid"));
                }
            }

            XElement states = Automaton.Element("initState");
            foreach (XElement child in states.Elements())
            {
                if (child.Name == "state")
                {
                    start = (int)child.Attribute("sid");
                }
            }

            return new Pair<HashSet<char>, Automaton<BDD>>(al, Automaton<BDD>.Create(start, finalStates, moves));

        }

        public static Automaton<BDD> parseForTest(string Automaton1, CharSetSolver solver)
        {           
            var moves = new List<Move<BDD>>();
            var finalStates = new List<int>();
            int start = 0;

            XElement Automaton = XElement.Parse(Automaton1);

            XElement trans = Automaton.Element("transitionSet");

            foreach (XElement child in trans.Elements())
            {
                if (child.Name == "transition")
                {
                    moves.Add(new Move<BDD>(Convert.ToInt32(child.Element("from").Value), Convert.ToInt32(child.Element("to").Value),
                        solver.MkCharConstraint(false, Convert.ToChar(child.Element("read").Value))));
                }
            }

            XElement acc = Automaton.Element("acceptingSet");
            foreach (XElement child in acc.Elements())
            {
                if (child.Name == "state")
                {
                    finalStates.Add((int)child.Attribute("sid"));
                }
            }

            XElement states = Automaton.Element("initState");
            foreach (XElement child in states.Elements())
            {
                if (child.Name == "state")
                {
                    start = (int)child.Attribute("sid");
                }
            }

            return Automaton<BDD>.Create(start, finalStates, moves);

        }

        public static Pair<HashSet<char>, Automaton<BDD>> parseRegexFromXML(XElement regex, XElement alphabet, CharSetSolver solver)
        {
            HashSet<char> al = new HashSet<char>();
            XElement xmlAlphabet = XElement.Parse(RemoveAllNamespaces(alphabet.ToString()));

            string alRex = "";
            bool first = true;
            foreach (XElement child in xmlAlphabet.Elements())
            {
                char element = Convert.ToChar(child.Value);
                al.Add(element);
                if (first)
                {
                    first = false;
                    alRex += element;
                }
                else
                {
                    alRex += "|"+element;
                }
            }

            XElement Regex = XElement.Parse(RemoveAllNamespaces(regex.ToString()));

            // Replacing epsilon placeholder with .NET equivalent expression for epsilon
            string rexpr = Regex.Value.Trim().toDotNet();

            var escapedRexpr = string.Format(@"^({0})$",rexpr);

            Automaton<BDD> aut = null;
            try
            {
                aut = solver.Convert(escapedRexpr).RemoveEpsilons(solver.MkOr).Determinize(solver);
                // Removing all transitions labeled with the character 'ε' (not actual epsilon transitions)
                var moves = aut.GetMoves();
                // Remeber to check with separate deletion in case it does not work
                var superflous = new HashSet<Move<BDD>>();
                foreach(var move in moves)
                {
                    if (solver.IsSatisfiable(solver.MkAnd(move.Label, solver.MkCharConstraint(false, 'ε'))))
                        superflous.Add(move);
                }
                foreach (var move in superflous) {
                    aut.RemoveTheMove(move);
                }
                aut = Automaton<BDD>.Create(aut.InitialState, aut.GetFinalStates().Intersect(aut.GetStates()), aut.GetMoves());
                aut.Determinize(solver);
            }
            catch (ArgumentException e)
            {
                throw new PDLException("The input is not a well formatted regular expression: "+e.Message);
            }
            catch (AutomataException e)
            {
                throw new PDLException("The input is not a well formatted regular expression: " + e.Message);
            }




            var diff = aut.Intersect(solver.Convert(@"^("+alRex+@")*$").Complement(solver), solver);
            if(!diff.IsEmpty)
                throw new PDLException(
                    "The regular expression should only accept strings over ("+alRex+")*. Yours accepts the string '"+DFAUtilities.GenerateShortTerm(diff.Determinize(solver),solver)+"'");

            return new Pair<HashSet<char>, Automaton<BDD>>(al, aut);

        }

        public static Pair<HashSet<char>, Automaton<BDD>> parseDFAFromJFLAP(string fileName, CharSetSolver solver)
        {
            
            HashSet<char> al = new HashSet<char>();
            XElement Structure = XElement.Load(fileName);

            XElement MType = Structure.Element("type");
            Debug.Assert(MType.Value == "fa");

            XElement Automaton = Structure.Element("automaton");

            var moves = new List<Move<BDD>>();
            var finalStates = new List<int>();
            int start = -1;

            foreach (XElement child in Automaton.Elements())
            {
                if (child.Name == "state") // make start and/or add to final
                {
                    foreach (XElement d in child.Elements())
                    {
                        if (d.Name == "initial")
                            start = (int)child.Attribute("id");
                        if (d.Name == "final")
                            finalStates.Add((int)child.Attribute("id"));
                    }
                            
                }
                if (child.Name == "transition")
                {
                    al.Add(Convert.ToChar(child.Element("read").Value));
                    moves.Add(new Move<BDD>(Convert.ToInt32(child.Element("from").Value), Convert.ToInt32(child.Element("to").Value),
                        solver.MkCharConstraint(false, Convert.ToChar(child.Element("read").Value))));
                }
            }

            Debug.Assert(start != -1);
            return new Pair<HashSet<char>, Automaton<BDD>>(al, Automaton<BDD>.Create(start, finalStates, moves));
        }

        //public static Pair<HashSet<char>, Automaton<BDD>> parseDFAfromTutor(string fileName, CharSetSolver solver)
        //{
        //    HashSet<char> al = new HashSet<char>();
        //    XElement Automaton = XElement.Load(fileName);

        //    //All DFAs in problem set on automata tutor are over a,b
        //    al.Add('a');
        //    al.Add('b');

        //    var moves = new List<Move<BDD>>();
        //    var finalStates = new List<int>();
        //    int start = 0;

        //    XElement trans = Automaton.Element("transitionSet");

        //    foreach (XElement child in trans.Elements())
        //    {
        //        if (child.Name == "transition")
        //        {
        //            moves.Add(new Move<BDD>(Convert.ToInt32(child.Element("from").Value), Convert.ToInt32(child.Element("to").Value),
        //                solver.MkCharConstraint(false, Convert.ToChar(child.Element("read").Value))));
        //        }
        //    }

        //    XElement acc = Automaton.Element("acceptingSet");
        //    foreach (XElement child in acc.Elements())
        //    {
        //        if (child.Name == "state")
        //        {
        //            finalStates.Add((int)child.Attribute("sid"));
        //        }
        //    }

        //    return new Pair<HashSet<char>, Automaton<BDD>>(al, Automaton<BDD>.Create(start, finalStates, moves));

        //}

        //public static Pair<HashSet<char>, Automaton<BDD>> parseDFAfromEvent(string fileName, CharSetSolver solver)
        //{
        //    HashSet<char> al = new HashSet<char>();
        //    XElement Event = XElement.Load(fileName);

        //    XElement Automaton = Event.Element("automaton");

        //    //All DFAs in problem set on automata tutor are over a,b
        //    al.Add('a');
        //    al.Add('b');

        //    var moves = new List<Move<BDD>>();
        //    var finalStates = new List<int>();
        //    int start = 0;

        //    XElement trans = Automaton.Element("transitionSet");

        //    foreach (XElement child in trans.Elements())
        //    {
        //        if (child.Name == "transition")
        //        {
        //            moves.Add(new Move<BDD>(Convert.ToInt32(child.Element("from").Value), Convert.ToInt32(child.Element("to").Value),
        //                solver.MkCharConstraint(false, Convert.ToChar(child.Element("read").Value))));
        //        }
        //    }

        //    XElement acc = Automaton.Element("acceptingSet");
        //    foreach (XElement child in acc.Elements())
        //    {
        //        if (child.Name == "state")
        //        {
        //            finalStates.Add((int)child.Attribute("sid"));
        //        }
        //    }

        //    return new Pair<HashSet<char>, Automaton<BDD>>(al, Automaton<BDD>.Create(start, finalStates, moves));

        //}

        public static bool IsEventEqual(string file1, string file2)
        {
            XElement Event1 = XElement.Load(file1);
            XElement Automaton1 = Event1.Element("automaton");

            XElement Event2 = XElement.Load(file2);
            XElement Automaton2 = Event2.Element("automaton");

            return Automaton1.ToString() == Automaton2.ToString();
        }

        public static void printDFA(Automaton<BDD> dfa, HashSet<char> alphabet, StringBuilder sb)
        {
            var newDfa = normalizeDFA(dfa).First;

            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

            sb.Append("alphabet:");
            foreach (var ch in alphabet)
                sb.Append(" " + ch);
            sb.AppendLine();

            sb.AppendLine(string.Format("{0} states", newDfa.StateCount));

            sb.Append("final states:");
            foreach (var st in newDfa.GetFinalStates())
                sb.Append(" " + st);
            sb.AppendLine();
            
            foreach (var move in newDfa.GetMoves())
            {

                List<char> chars = move.Label==null? new List<char>() : solver.GenerateAllCharacters(move.Label, false).ToList();
                chars.Sort();
                foreach (var ch in chars)
                {
                    sb.AppendLine(string.Format("{0},{1},{2}", move.SourceState, move.TargetState, ch));
                }
            }
        }

        //public static Pair<Automaton<BDD>, HashSet<char>> readDFA(string )
        //{            
        //    CharSetSolver solver = new CharSetSolver(BitWidth.BV64);


        //    sb.Append("alphabet:");
        //    foreach (var ch in alphabet)
        //        sb.Append(" " + ch);
        //    sb.AppendLine();

        //    sb.AppendLine(string.Format("{0} states", newDfa.StateCount));

        //    sb.Append("final states:");
        //    foreach (var st in newDfa.GetFinalStates())
        //        sb.Append(" " + st);
        //    sb.AppendLine();

        //    foreach (var move in newDfa.GetMoves())
        //    {
        //        var chars = solver.GenerateAllCharacters(move.Label, false).ToList();
        //        chars.Sort();
        //        foreach (var ch in chars)
        //        {
        //            sb.AppendLine(string.Format("{0},{1},{2}", move.SourceState, move.TargetState, ch));
        //        }
        //    }
        //}

        #region XML parsing helpers
        public static string RemoveAllNamespaces(string xmlDocument)
        {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

            return xmlDocumentWithoutNs.ToString();
        }

        public static XElement RemoveJustNamespaces(XElement xmlDocument)
        {
            XElement xElement = new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveJustNamespaces(el)));

            if (!xmlDocument.HasElements)
                xElement.Value = xmlDocument.Value;
            
            foreach (XAttribute attribute in xmlDocument.Attributes())
            {
                if (!attribute.IsNamespaceDeclaration)
                {
                    xElement.SetAttributeValue(attribute.Name, attribute.Value);
                }
            }

            return xElement;
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                {
                    if (!attribute.IsNamespaceDeclaration)
                    {
                        xElement.Add(attribute);
                    }
                }

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }
        #endregion

        #region canonical state names
        public static Pair<Automaton<BDD>,Dictionary<int,int>> normalizeDFA(Automaton<BDD> dfa)
        {
            Dictionary<int, int> stateToNewNames = new Dictionary<int, int>();
            Dictionary<int, int> newNamesToStates = new Dictionary<int, int>();
            int i = 0;
            foreach (var state in dfsStartingTimes(dfa))
            {
                newNamesToStates[i] = state;
                stateToNewNames[state] = i;
                i++;
            }

            var oldFinalStates = dfa.GetFinalStates();
            var newFinalStates = new List<int>();
            foreach (var st in oldFinalStates)
                newFinalStates.Add(stateToNewNames[st]);

            var oldMoves = dfa.GetMoves();
            var newMoves = new List<Move<BDD>>();
            foreach (var move in oldMoves)
                newMoves.Add(new Move<BDD>(stateToNewNames[move.SourceState], stateToNewNames[move.TargetState], move.Label));

            return new Pair<Automaton<BDD>, Dictionary<int, int>>(Automaton<BDD>.Create(0, newFinalStates, newMoves), newNamesToStates);
        }

        private static List<int> dfsStartingTimes(Automaton<BDD> dfa)
        {
            List<int> order = new List<int>();

            HashSet<int> discovered = new HashSet<int>();
            discovered.Add(dfa.InitialState);

            dfsRecStartingTimes(dfa, dfa.InitialState, discovered, order);

            //Deal with states not reachable from init
            foreach (var state in dfa.States)
                if (!discovered.Contains(state))
                {
                    discovered.Add(state);
                    dfsRecStartingTimes(dfa, state, discovered, order);
                }

            return order;
        }

        private static void dfsRecStartingTimes(Automaton<BDD> dfa, int currState, HashSet<int> discovered, List<int> order)
        {
            order.Add(currState);
            List<Move<BDD>> moves = new List<Move<BDD>>(dfa.GetMovesFrom(currState));
            moves.Sort(delegate(Move<BDD> c1, Move<BDD> c2) {
                if(c1.Label==null)
                    return 1;
                if (c2.Label == null)
                    return -1;
                return c1.Label.ToString().CompareTo(c2.Label.ToString());
            });
            foreach (var move in moves)
                if (!discovered.Contains(move.TargetState))
                {
                    discovered.Add(move.TargetState);
                    dfsRecStartingTimes(dfa, move.TargetState, discovered, order);
                }
        }
        #endregion

        #region TestSet Generation with Myhill-Nerode
        //returns a pair of string enumerable of positive and negative test set respectively
        internal static Pair<IEnumerable<string>, IEnumerable<string>> MyHillTestGeneration(HashSet<char> alphabet, Automaton<BDD> dfa, CharSetSolver solver)
        {
            Automaton<BDD> normDfa = normalizeDFA(dfa).First;                       

            HashSet<string> pos = new HashSet<string>();
            HashSet<string> neg = new HashSet<string>();

            Automaton<BDD> ait, bif, bjf, adif;
            HashSet<string> testSet = new HashSet<string>();
            var finStates = normDfa.GetFinalStates();

            string[] a = new string[normDfa.StateCount];
            string[,] b = new string[normDfa.StateCount, normDfa.StateCount];

            #region Compute ai and bij
            foreach (var state1 in normDfa.States)
            {
                ait = Automaton<BDD>.Create(normDfa.InitialState, new int[] { state1 }, normDfa.GetMoves());
                a[state1] = GenerateShortTerm(ait, solver);

                bif = Automaton<BDD>.Create(state1, finStates, normDfa.GetMoves());

                foreach (var state2 in normDfa.States)
                {
                    bjf = Automaton<BDD>.Create(state2, finStates, new List<Move<BDD>>(normDfa.GetMoves()));

                    adif = bif.Minus(bjf, solver).Determinize(solver).Minimize(solver);

                    b[state1, state2] = GenerateShortTerm(adif, solver);
                }
            }
            #endregion

            for (int i = 0; i < normDfa.StateCount; i++)
                for (int j = 0; j < normDfa.StateCount; j++)
                {
                    if (b[i, j] != null)
                        pos.Add(a[i] + b[i, j]);
                    if (b[j, i] != null)
                        neg.Add(a[i] + b[j, i]);
                    foreach (char c in alphabet)
                    {
                        int new_i = GetNextState(i, c, normDfa, solver);
                        if (new_i!=-1 && b[new_i, j] != null)
                            pos.Add(a[i] + c + b[new_i, j]);
                        if (new_i != -1 && b[j, new_i] != null)
                            neg.Add(a[i] + c + b[j, new_i]);
                    }
                }

            return new Pair<IEnumerable<string>, IEnumerable<string>>(pos, neg);
        }

        // returns the state reached from currState when reading c
        private static int GetNextState(int currState, char c, Automaton<BDD> dfa, CharSetSolver solver)
        {
            foreach (var move in dfa.GetNonepsilonMovesFrom(currState))
                if (solver.IsSatisfiable(solver.MkAnd(move.Label, solver.MkCharConstraint(false, c))))
                    return move.TargetState;

            return -1;
        }
        private static HashSet<State<string>> GetNextNfaStates(int currState, char c, Automaton<BDD> nfa, CharSetSolver solver)
        {
            var ret = new HashSet<State<string>>();
            foreach (var move in nfa.GetNonepsilonMovesFrom(currState))
                if (solver.IsSatisfiable(solver.MkAnd(move.Label, solver.MkCharConstraint(false, c))))
                    ret.Add(new State<string>(move.TargetState.ToString()));

            return ret;
        }

        public static int GetStateAfterString(int currState, string word, Automaton<BDD> dfa, CharSetSolver solver)
        {
            int state = currState;
            foreach (var letter in word)
            {
                if (state == -1)
                    return -1;
                state = GetNextState(state, letter, dfa, solver);
            }
            return state;
        }

        // Automaton must be first minimized before passing it into the function
        private static Dictionary<int, string> GetEquivalenceClasses(Automaton<BDD> minimalDfa, HashSet<char> alphabet, CharSetSolver solver)
        {
            Dictionary<int, string> rep = new Dictionary<int, string>();
            HashSet<int> visited = new HashSet<int>();
            Queue<int> queue = new Queue<int>();

            rep.Add(minimalDfa.InitialState, "");
            queue.Enqueue(minimalDfa.InitialState);
            visited.Add(minimalDfa.InitialState);

            while(queue.Count != 0)
            {
                int state = queue.Dequeue();
                string shortest = rep[state];
                foreach(var letter in alphabet)
                {
                    var next = GetNextState(state, letter, minimalDfa, solver);
                    if (!visited.Contains(next))
                    {
                        visited.Add(next);
                        rep.Add(next, shortest + letter);
                        if(next != -1)
                            queue.Enqueue(next);
                    }
                }
            }
            rep[minimalDfa.InitialState] = "";

            return rep;
        }

        // Automaton must be first minimized before passing it into the function
        public static string GetRepresentative(int targetState, Automaton<BDD> minimalDfa, HashSet<char> alphabet, CharSetSolver solver)
        {
            var rep = GetEquivalenceClasses(minimalDfa, alphabet, solver);
            return rep[targetState];
        }

        // Only to be called if the source states are not equivalent
        // return : the first item of the pair is a success flag
        public static Pair<bool, string> GetDifferentiatingWord(int source1, int source2, Automaton<BDD> dfa, HashSet<char> alphabet, CharSetSolver solver)
        {
            HashSet<Pair<int, int>> visited = new HashSet<Pair<int, int>>();
            Queue<Tuple<int, int, string>> queue = new Queue<Tuple<int, int, string>>();

            queue.Enqueue(new Tuple<int, int, string>(source1, source2, ""));
            visited.Add(new Pair<int, int>(source1, source2));
            var finals = dfa.GetFinalStates();

            while(queue.Count > 0)
            {
                var t = queue.Dequeue();
                bool c1 = finals.Contains(t.Item1);
                bool c2 = finals.Contains(t.Item2);
                if (c1 ^ c2)
                    return new Pair<bool, string>(true, t.Item3);
                foreach(var letter in alphabet)
                {
                    var state1 = t.Item1 != -1 ? GetNextState(t.Item1, letter, dfa, solver) : -1;
                    var state2 = t.Item2 != -1 ? GetNextState(t.Item2, letter, dfa, solver) : -1;
                    var p = new Pair<int, int>(state1, state2);
                    if (!visited.Contains(p))
                    {
                        visited.Add(p);
                        queue.Enqueue(new Tuple<int, int, string>(state1, state2, t.Item3 + letter));
                    }
                }
            }
            return new Pair<bool, string>(false, "");
        }

        internal static string GenerateShortTerm(Automaton<BDD> dfa, CharSetSolver solver)
        {
            if (dfa.IsEmpty)
                return null;

            Dictionary<int, string> shortStr = new Dictionary<int, string>();

            HashSet<int> reachedStates = new HashSet<int>();
            List<int> toExplore = new List<int>();


            reachedStates.Add(dfa.InitialState);
            toExplore.Add(dfa.InitialState);
            shortStr.Add(dfa.InitialState, "");
            var finSts = dfa.GetFinalStates();
            if (finSts.Contains(dfa.InitialState))
                return "";

            string sCurr = ""; char condC = 'a';
            while (toExplore.Count != 0)
            {
                var current = toExplore.First();
                toExplore.RemoveAt(0);
                shortStr.TryGetValue(current, out sCurr);

                var reachableFromCurr = dfa.GetMovesFrom(current);
                foreach (var move in reachableFromCurr)
                {
                    if (!reachedStates.Contains(move.TargetState))
                    {
                        reachedStates.Add(move.TargetState);
                        toExplore.Add(move.TargetState);

                        if (move.Label == null)
                        {
                            shortStr.Add(move.TargetState, sCurr);
                        }
                        else
                        {
                            foreach (var v in solver.GenerateAllCharacters(move.Label, false))
                            {
                                condC = v;
                                break;
                            }
                            shortStr.Add(move.TargetState, sCurr + condC);
                        }
                        if (finSts.Contains(move.TargetState))
                        {
                            return sCurr + condC;
                        }

                    }
                }
            }
            return null;
        }
        #endregion

        #region TestSet Generation with enumeration

        public static Pair<List<string>, List<string>> GetTestSets(
            Automaton<BDD> dfa, HashSet<char> alphabet, CharSetSolver solver)
        {
            List<string> positive = new List<string>();
            List<string> negative = new List<string>();
            var finalStates = dfa.GetFinalStates().ToList();

            ComputeModels("", dfa.InitialState, dfa, finalStates, alphabet, solver, positive, negative);

            positive.Sort();
            negative.Sort();
            return new Pair<List<string>, List<string>>(positive, negative);
        }

        internal static void ComputeModels(
            string currStr, int currState,
            Automaton<BDD> dfa, List<int> finalStates, HashSet<char> alphabet, CharSetSolver solver,
            List<string> positive, List<string> negative)
        {
            if (currStr.Length >= 8)
                return;

            if (currState == -1 || !finalStates.Contains(currState))
                negative.Add(currStr);
            else
                positive.Add(currStr);

            foreach (char ch in alphabet)
            {
                if (currState == -1)
                    ComputeModels(currStr + ch, currState, dfa, finalStates, alphabet, solver, positive, negative);
                else
                {
                    bool found = false;
                    foreach (var move in dfa.GetMovesFrom(currState))
                    {
                        if (solver.IsSatisfiable(solver.MkAnd(move.Label, solver.MkCharConstraint(false, ch))))
                        {
                            found = true;
                            ComputeModels(currStr + ch, move.TargetState, dfa, finalStates, alphabet, solver, positive, negative);
                            break;
                        }
                    }
                    if (!found)
                        ComputeModels(currStr + ch, -1, dfa, finalStates, alphabet, solver, positive, negative);
                }
            }
        }
        #endregion


        //Compute strings in cycles
        #region Compute strings in cycles
        internal static HashSet<string> getLoopingStrings(Automaton<BDD> dfa, HashSet<Char> al, CharSetSolver solver)
        {
            var cycles = getSimpleCycles(dfa);
            HashSet<string> strings = new HashSet<string>();
            foreach (var cycle in cycles)
            {
                var state = cycle.ElementAt(0);
                cycle.RemoveAt(0);
                getPathStrings(dfa, solver, cycle, "", strings, state);
            }
            return strings;
        }

        // gets string that forms the loop path given as a list of states
        private static void getPathStrings(Automaton<BDD> dfa, CharSetSolver solver, List<int> path, string currStr, HashSet<string> strings, int prevState)
        {
            List<int> path1 = new List<int>(path);
            var currState = path1.ElementAt(0);
            path1.RemoveAt(0);
            
            foreach (var move in dfa.GetMovesFrom(prevState))
                if (move.TargetState == currState)
                {
                    if (move.Label == null)
                    {
                        if (path1.Count == 0)
                            strings.Add(currStr);
                        else
                            getPathStrings(dfa, solver, path1, currStr, strings, currState);
                    }
                    else
                        foreach (char c in solver.GenerateAllCharacters(move.Label, false))
                            if (path1.Count == 0)
                                strings.Add(currStr + c);
                            else
                                getPathStrings(dfa, solver, path1, currStr + c, strings, currState);
                }
                    
        }
        #endregion

        // Accessory methods for SCC and cycles
        #region Accessory methods for SCC and cycles
        internal static HashSet<int> getCyclesLengths(Automaton<BDD> dfa)
        {
            HashSet<int> lengths = new HashSet<int>();
            var sccs = computeSCC(dfa);
            foreach (var scc in sccs)
            {
                HashSet<int>[] dic = new HashSet<int>[scc.Count + 1];

                foreach (var state in scc)
                {
                    for (int i = 1; i <= scc.Count; i++)
                        dic[i] = new HashSet<int>();
                    getCyclesLengthsFromNode(1, dfa, state, dic, scc.Count);
                    for (int i = 1; i <= scc.Count; i++)
                        if (dic[i].Contains(state))
                            lengths.Add(i);
                }
            }
            return lengths;
        }

        private static void getCyclesLengthsFromNode(int length,
            Automaton<BDD> dfa, int currState, HashSet<int>[] found, int max)
        {
            if (length <= max)
                foreach (var move in dfa.GetMovesFrom(currState))
                {
                    if (!found[length].Contains(move.TargetState))
                    {
                        found[length].Add(move.TargetState);
                        getCyclesLengthsFromNode(length + 1, dfa, move.TargetState, found, max);
                    }
                }
        }

        private static List<int> dfsFinishingTimes(Automaton<BDD> dfa)
        {
            List<int> order = new List<int>();

            HashSet<int> discovered = new HashSet<int>();
            discovered.Add(dfa.InitialState);

            dfsRecFinishingTimes(dfa, dfa.InitialState, discovered, order);
            //Deal with states not reachable from init
            foreach (var state in dfa.States)
                if (!discovered.Contains(state))
                {
                    discovered.Add(state);
                    dfsRecFinishingTimes(dfa, state, discovered, order);
                }

            return order;
        }

        private static void dfsRecFinishingTimes(Automaton<BDD> dfa, int currState, HashSet<int> discovered, List<int> order)
        {
            foreach (var move in dfa.GetMovesFrom(currState))
            {
                if (!discovered.Contains(move.TargetState))
                {
                    discovered.Add(move.TargetState);
                    dfsRecFinishingTimes(dfa, move.TargetState, discovered, order);
                }
            }
            order.Insert(0, currState);
        }


        internal static List<HashSet<int>> computeSCC(Automaton<BDD> dfa)
        {
            List<int> order = dfsFinishingTimes(dfa);

            var list = new List<HashSet<int>>();

            HashSet<int> discovered = new HashSet<int>();
            HashSet<int> comp = new HashSet<int>();

            while (order.Count > 0)
            {
                discovered.Add(order.ElementAt(0));
                backwardDfsRec(dfa, order.ElementAt(0), discovered, comp);
                list.Add(comp);
                foreach (var item in comp)
                    order.Remove(item);
                comp = new HashSet<int>();
            }

            return list;
        }

        private static void backwardDfsRec(Automaton<BDD> dfa, int currState, HashSet<int> discovered, HashSet<int> comp)
        {
            foreach (var move in dfa.GetMovesTo(currState))
            {
                if (!discovered.Contains(move.SourceState))
                {
                    discovered.Add(move.SourceState);
                    backwardDfsRec(dfa, move.SourceState, discovered, comp);
                }
            }
            comp.Add(currState);
        }

        #endregion

        // Accessory methods for strings
        #region Accessory methods for strings
        internal static List<string> getSimplePrefixes(Automaton<BDD> dfa, CharSetSolver solver)
        {
            if (!dfa.IsEpsilonFree)
                return new List<string>();

            List<string> strings = new List<string>();
            foreach (var path in getSimplePaths(dfa))
            {
                var currStrs = new List<string>();
                currStrs.Add("");
                var p = new List<int>(path);
                p.RemoveAt(0);
                int prevNode = dfa.InitialState;
                foreach (int node in p)
                {
                    foreach (var move in dfa.GetMovesFrom(prevNode))
                    {
                        if (node == move.TargetState)
                        {
                            var newStrs = new List<string>();
                            foreach (var el in solver.GenerateAllCharacters(move.Label, false))
                                foreach (var str in currStrs)
                                {
                                    newStrs.Add(str + el);
                                    strings.Add(str + el);
                                }
                            currStrs = new List<string>(newStrs);
                            break;
                        }
                    }
                    prevNode = node;
                }

            }
            return strings;
        }

        internal static List<string> getSimpleSuffixes(Automaton<BDD> dfa, CharSetSolver solver)
        {
            if (!dfa.IsEpsilonFree)
                return new List<string>();

            List<string> strings = new List<string>();
            foreach (var path in getSimplePaths(dfa))
            {
                var currStrs = new List<string>();
                currStrs.Add("");
                var p = new List<int>(path);
                p.Reverse();
                int prevNode = p.ElementAt(0);
                p.RemoveAt(0);

                foreach (int node in p)
                {
                    foreach (var move in dfa.GetMovesTo(prevNode))
                    {
                        if (node == move.SourceState)
                        {
                            var newStrs = new List<string>();
                            foreach (var el in solver.GenerateAllCharacters(move.Label, false))
                                foreach (var str in currStrs)
                                {
                                    newStrs.Add(el + str);
                                    strings.Add(el + str);
                                }
                            currStrs = new List<string>(newStrs);
                            break;
                        }
                    }
                    prevNode = node;
                }

            }
            return strings;

        }

        internal static List<List<int>> getSimplePaths(Automaton<BDD> dfa)
        {
            if (!dfa.IsEpsilonFree)
                return new List<List<int>>();

            List<List<int>> paths = new List<List<int>>();
            var cp = new List<int>();
            cp.Add(dfa.InitialState);
            getSimplePathsDFS(dfa.InitialState, dfa, paths, cp);
            return paths;
        }

        internal static void getSimplePathsDFS(int currState, Automaton<BDD> dfa, List<List<int>> paths, List<int> currPath)
        {
            foreach (var move in dfa.GetMovesFrom(currState))
            {
                if (!currPath.Contains(move.TargetState))
                {
                    var npath = new List<int>(currPath);
                    npath.Add(move.TargetState);
                    if (dfa.GetFinalStates().Contains(move.TargetState))
                        paths.Add(npath);
                    getSimplePathsDFS(move.TargetState, dfa, paths, npath);
                }
            }

        }

        internal static List<List<int>> getSimpleCycles(Automaton<BDD> dfa)
        {
            if (!dfa.IsEpsilonFree)
                return new List<List<int>>();

            List<List<int>> cycles = new List<List<int>>();
            foreach (var state in dfa.States)
            {
                var cp = new List<int>();
                cp.Add(state);
                getSimpleCyclesDFS(state, dfa, cycles, cp);
            }
            return cycles;
        }

        internal static void getSimpleCyclesDFS(int currState, Automaton<BDD> dfa, List<List<int>> cycles, List<int> currPath)
        {
            foreach (var move in dfa.GetMovesFrom(currState))
            {
                if (!currPath.Contains(move.TargetState))
                {
                    var npath = new List<int>(currPath);
                    npath.Add(move.TargetState);
                    getSimpleCyclesDFS(move.TargetState, dfa, cycles, npath);
                }
                else
                {
                    if (currPath.ElementAt(0) == move.TargetState)
                    {
                        var npath = new List<int>(currPath);
                        npath.Add(move.TargetState);
                        cycles.Add(npath);
                    }
                }
            }

        }
        #endregion        

        // Return true iff dfa2 behaves correctly on all the inputs the pair (pos,neg)
        // To be used only when testing again same dfa1 over and over
        internal static bool ApproximateMNEquivalent(
            Pair<IEnumerable<string>, IEnumerable<string>> testSets, 
            double lanDensity,
            Automaton<BDD> shouldbeDfa, HashSet<char> al, CharSetSolver solver)
        {
            var dfa = shouldbeDfa;
            if(!shouldbeDfa.isDeterministic || !shouldbeDfa.IsEpsilonFree)
                dfa = shouldbeDfa.RemoveEpsilons(solver.MkOr).Determinize(solver).MakeTotal(solver).Minimize(solver);
                
            //Check against test cases
            var positive = testSets.First;
            var negative = testSets.Second;

            if (lanDensity < 0.5)
            {
                foreach (var s in positive)
                    if (!Accepts(dfa, s, al, solver))
                        return false;

                foreach (var s in negative)
                    if (Accepts(dfa, s, al, solver))
                        return false;
            }
            else
            {
                foreach (var s in negative)
                    if (Accepts(dfa, s, al, solver))
                        return false;

                foreach (var s in positive)
                    if (!Accepts(dfa, s, al, solver))
                        return false;
            }

            return true;
        }

        //returns true iff dfa accepts str
        private static bool Accepts(Automaton<BDD> dfa1,  string str, HashSet<char> al, CharSetSolver solver)
        {
            return solver.Accepts(dfa1, str);

            //int currState = dfa1.InitialState;
            //for (int i = 0; i < str.Length; i++)
            //{
            //    currState = GetNextState(currState, str[i], dfa1, solver);
            //    if (currState < 0)
            //        return false;
            //}
            //return dfa1.GetFinalStates().ToList().Contains(currState);
        }

    }
}
