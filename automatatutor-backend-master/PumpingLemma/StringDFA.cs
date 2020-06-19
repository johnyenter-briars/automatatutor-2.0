using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using AutomataPDL.Automata;

namespace PumpingLemma
{
    // XML Serialization

    [Serializable]
    [XmlRoot("automaton", Namespace = "http://automatagrader.com/")]
    public struct AutomatonXML
    {
        [XmlArray("alphabet")]
        [XmlArrayItem("symbol")]
        public string[] alphabet;

        [XmlArray("stateSet")]
        [XmlArrayItem("state")]
        public StateXML[] states;

        [XmlArray("transitionSet")]
        [XmlArrayItem("transition")]
        public TransitionXML[] transitions;

        [XmlArray("acceptingSet")]
        [XmlArrayItem("state")]
        public SimpleStateXML[] acceptingStates;

        [XmlArray("initState")]
        [XmlArrayItem("state")]
        public SimpleStateXML[] initState;
    }

    [Serializable]
    public struct StateXML
    {
        [XmlAttribute("sid")]
        public string sid;

        [XmlElement("label")]
        public int label;

        [XmlElement("posX")]
        public int posX;

        [XmlElement("posY")]
        public int posY;
    }

    [Serializable]
    public struct SimpleStateXML
    {
        [XmlAttribute("sid")]
        public string sid;
    }

    [Serializable]
    public struct TransitionXML
    {
        [XmlAttribute("tid")]
        public string tid;

        [XmlElement("from")]
        public int from;
        [XmlElement("to")]
        public int to;
        [XmlElement("read")]
        public string read;
        [XmlElement("edgeDistance")]
        public int edgeDistance;
    }


    //Wrapper for DFA
    public class StringDFA
    {
        public string q_0;
        public HashSet<string> F;
        public Dictionary<TwoTuple<string, string>, string> delta;
        public HashSet<string> states;
        public HashSet<string> alphabet;

        public StringDFA(DFA<string, HashSet<State<Set<State<string>>>>> dfa)
        {
            q_0 = setToString(dfa.q_0);

            var temp_states = new HashSet<string>();
            foreach (var state in dfa.Q)
            {
                temp_states.Add(setToString(state));
            }
            var temp_states_ordered = temp_states.OrderBy(x => x);
            Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add("-1", "-1");
            int ctr = 0;
            states = new HashSet<string>();
            foreach(var state in temp_states_ordered)
            {
                if (state != "-1")
                {
                    map.Add(state, ctr+"");
                    states.Add(ctr + "");
                    ctr++;
                }
                else
                {
                    states.Add("-1");
                }
            }

            F = new HashSet<string>();
            foreach (var state in dfa.F)
            {
                F.Add(map[setToString(state)]);
            }

            delta = new Dictionary<TwoTuple<string, string>, string>();
            foreach (var trans in dfa.delta)
            {
                delta.Add(new TwoTuple<string, string>(map[setToString(trans.Key.first)], trans.Key.second), map[setToString(trans.Value)]);
            }

            alphabet = dfa.Sigma;
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;
                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);
                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        public StringDFA(XElement automatonXML, bool deterministic)
        {
            AutomatonXML aXML;
            var xmlSerializer = new XmlSerializer(typeof(AutomatonXML));
            var doc = new XDocument();
            doc.Add(automatonXML);
            using (var reader = doc.CreateReader())
            {
                aXML = (AutomatonXML)xmlSerializer.Deserialize(reader);
            }

            if (deterministic)
            {
                this.q_0 = aXML.initState[0].sid;
                this.F = new HashSet<string>();
                foreach (var state in aXML.acceptingStates)
                {
                    this.F.Add(state.sid);
                }

                this.delta = new Dictionary<TwoTuple<string, string>, string>();
                foreach (var transition in aXML.transitions)
                {
                    delta.Add(new TwoTuple<string, string>(transition.from + "", transition.read), transition.to + "");
                }

                this.states = new HashSet<string>();
                foreach (var state in aXML.states)
                {
                    this.states.Add(state.sid);
                }

                this.alphabet = new HashSet<string>();
                foreach (string letter in aXML.alphabet)
                {
                    alphabet.Add(letter);
                }
            }

            else
            {
                //NFA
                HashSet<State<string>> states = new HashSet<State<string>>();
                foreach (var state in aXML.states)
                {
                    if (Int32.TryParse(state.sid, out int parsed))
                        states.Add(new State<string>(parsed, state.sid));
                    else
                        states.Add(new State<string>(0, state.sid));
                }
                HashSet<string> alphabet = new HashSet<string>();
                foreach (var letter in aXML.alphabet)
                {
                    alphabet.Add(letter);
                }
                HashSet<TwoTuple<State<string>, State<string>>> epsilonTransitions = new HashSet<TwoTuple<State<string>, State<string>>>();
                Dictionary<TwoTuple<State<string>, string>, HashSet<State<string>>> delta = new Dictionary<TwoTuple<State<string>, string>, HashSet<State<string>>>();
                foreach (var transition in aXML.transitions)
                {
                    if (transition.read.Equals("epsilon"))
                    {
                        epsilonTransitions.Add(new TwoTuple<State<string>, State<string>>(new State<string>(transition.from + ""), new State<string>(transition.to + "")));
                    }
                    else
                    {
                        TwoTuple<State<string>, string> twoTuple = new TwoTuple<State<string>, string>(new State<string>(transition.from + ""), transition.read);
                        HashSet<State<string>> set;
                        if (delta.TryGetValue(twoTuple, out set))
                            set.Add(new State<string>(transition.to + ""));
                        else
                        {
                            set = new HashSet<State<string>>();
                            set.Add(new State<string>(transition.to + ""));
                            delta.Add(new TwoTuple<State<string>, string>(new State<string>(transition.from + ""), transition.read), set);
                        }
                    }
                }
                HashSet<State<string>> Q_0 = new HashSet<State<string>>();
                foreach (var state in aXML.initState)
                {
                    Q_0.Add(new State<string>(state.sid));
                }
                HashSet<State<string>> F = new HashSet<State<string>>();
                foreach (var state in aXML.acceptingStates)
                {
                    F.Add(new State<string>(state.sid));
                }

                var nfa = collectEpsilonTransitions(states, alphabet, delta, Q_0, F, epsilonTransitions);
                
                var dfa = nfa.NFAtoDFA();
                var dfa_min = dfa.MinimizeHopcroft();
                var strDFA = new StringDFA(dfa_min);

                //rename error set
                foreach(var state in strDFA.states)
                {
                    if (!strDFA.F.Contains(state))
                    {
                        bool errorStateFound = true;
                        foreach (var letter in strDFA.alphabet)
                        {
                            if (!strDFA.delta[new TwoTuple<string, string>(state, letter)].Equals(state))
                            {
                                errorStateFound = false;
                                break;
                            }
                        }
                        if (errorStateFound)
                        {
                            strDFA.states.Remove(state);
                            strDFA.states.Add("-1");
                            if (strDFA.q_0.Equals(state))
                            {
                                strDFA.q_0 = "-1";
                            }
                            
                            HashSet<KeyValuePair<TwoTuple<string, string>, string>> entriesToChange = new HashSet<KeyValuePair<TwoTuple<string, string>, string>>();
                            foreach (var entry in strDFA.delta)
                            {
                                if (entry.Value.Equals(state))
                                {
                                    entriesToChange.Add(entry);
                                }
                            }
                            foreach (var entry in entriesToChange)
                            {
                                strDFA.delta[entry.Key] = "-1";
                            }

                            foreach (var letter in strDFA.alphabet)
                            {
                                strDFA.delta.Remove(new TwoTuple<string, string>(state, letter));
                                strDFA.delta.Add(new TwoTuple<string, string>("-1", letter), "-1");
                            }
                            break;
                        }
                    }
                }


                this.alphabet = strDFA.alphabet;
                this.delta = strDFA.delta;
                this.F = strDFA.F;
                this.q_0 = strDFA.q_0;
                this.states = strDFA.states;
            }

        }

        private State<string> newState(string state)
        {
            return new State<string>(Int32.Parse(state), state);
        }

        public DFA<string, string> ToDFA()
        {
            var Q = new HashSet<State<string>>();
            foreach (string state in states)
            {
                Q.Add(newState(state));
            }

            var delta_in = new Dictionary<TwoTuple<State<string>, string>, State<string>>();
            foreach (var trans in delta)
            {
                delta_in.Add(new TwoTuple<State<string>, string>(newState(trans.Key.first), trans.Key.second), newState(trans.Value));
            }

            var F_in = new HashSet<State<string>>();
            foreach (string state in F)
            {
                F_in.Add(newState(state));
            }

            return new DFA<string, string>(Q, alphabet, delta_in, newState(q_0), F_in);
        }

        public XElement ToXML(string[] alphabet)
        {
            Random rand = new Random();
            AutomatonXML automatonXML = new AutomatonXML();

            List<string> symbols = new List<string>();
            foreach (string letter in alphabet)
            {
                symbols.Add(letter);
            }
            automatonXML.alphabet = symbols.ToArray<string>();

            List<StateXML> stateXMLs = new List<StateXML>();
            foreach (string state in states)
            {
                StateXML stateXML = new StateXML();
                stateXML.label = Int32.Parse(state);
                stateXML.sid = state;
                stateXML.posX = rand.Next(200, 700);
                stateXML.posY = rand.Next(200, 500);
                stateXMLs.Add(stateXML);
            }
            automatonXML.states = stateXMLs.ToArray<StateXML>();

            List<TransitionXML> transitionXMLs = new List<TransitionXML>();
            foreach (var trans in delta)
            {
                TransitionXML transXML = new TransitionXML();
                transXML.from = Int32.Parse(trans.Key.first);
                transXML.to = Int32.Parse(trans.Value);
                transXML.read = trans.Key.second;
                transXML.edgeDistance = 30;
                transitionXMLs.Add(transXML);
            }
            automatonXML.transitions = transitionXMLs.ToArray<TransitionXML>();

            List<SimpleStateXML> acceptingStateXMLs = new List<SimpleStateXML>();
            foreach (string state in F)
            {
                SimpleStateXML ssXML = new SimpleStateXML { sid = state };
                acceptingStateXMLs.Add(ssXML);
            }
            automatonXML.acceptingStates = acceptingStateXMLs.ToArray<SimpleStateXML>();

            SimpleStateXML[] initState = new SimpleStateXML[1];
            initState[0] = new SimpleStateXML { sid = q_0 };
            automatonXML.initState = initState;

            XmlSerializer serializer = new XmlSerializer(typeof(AutomatonXML));
            XDocument doc = new XDocument();
            using (var writer = doc.CreateWriter())
            {
                serializer.Serialize(writer, automatonXML);
            }
            return doc.Root;
        }


        private string setToString(State<HashSet<State<Set<State<string>>>>> set)
        {
            StringBuilder sb = new StringBuilder();
            var l = set.label;
            foreach (var lab in l)
            {
                var l2 = lab.label;
                foreach (var lab2 in l2.content)
                {
                    sb.Append(lab2.label + "");
                }
            }
            if (sb.Length == 0) return "-1";
            return sb.ToString();
        }

        private List<string> wordToList(string word)
        {
            List<string> lis = new List<string>();
            char[] chars = word.ToCharArray();
            foreach (char c in chars)
            {
                lis.Add(c + "");
            }
            return lis;
        }

        public bool Accepts(string word)
        {
            return ToDFA().Accepts(wordToList(word));
        }

        public string getStateFromState(string word, string currentState)
        {
            foreach (char c in word)
            {
                currentState = delta[new TwoTuple<string, string>(currentState, c + "")];
            }
            return currentState;
        }

        /* UTILITY FOR NFA */

        public static NFA<string, string> collectEpsilonTransitions(HashSet<State<string>> states, HashSet<string> alphabet, Dictionary<TwoTuple<State<string>, string>, HashSet<State<string>>> delta, HashSet<State<string>> Q_0, HashSet<State<string>> F, HashSet<TwoTuple<State<string>, State<string>>> epsilonTransitions)
        {
            //collect epsilon Transitions

            //delete self transitions
            var toDelete = new HashSet<TwoTuple<State<string>, State<string>>>(epsilonTransitions.Where(t => t.first.Equals(t.second)));
            epsilonTransitions = new HashSet<TwoTuple<State<string>, State<string>>>(epsilonTransitions.Except(toDelete.AsEnumerable()));

            //transitive closure
            HashSet<TwoTuple<State<string>, State<string>>> newTrans = new HashSet<TwoTuple<State<string>, State<string>>>();
            do
            {
                epsilonTransitions.UnionWith(newTrans);

                foreach (State<string> state in states)
                {
                    var incoming = epsilonTransitions.Where(t => t.second.Equals(state));
                    var outgoing = epsilonTransitions.Where(t => t.first.Equals(state));

                    foreach (var i in incoming)
                    {
                        foreach (var o in outgoing)
                        {
                            newTrans.Add(new TwoTuple<State<string>, State<string>>(i.first, o.second));
                        }
                    }
                }

            }
            while (newTrans.Except(epsilonTransitions).Any());

            //incoming epsilon transitions
            foreach (var stateTrans in epsilonTransitions)
            {
                foreach (string letter in alphabet)
                {
                    HashSet<State<string>> transitionTo;
                    if (delta.TryGetValue(new TwoTuple<State<string>, string>(stateTrans.second, letter), out transitionTo))
                    {

                        foreach (var stateTo in transitionTo)
                        {
                            addTransition(stateTrans.first, letter, stateTo, delta);
                        }

                    }
                }
            }

            //outgoing epsilon transitions
            foreach (var trans in delta)
            {
                foreach (State<string> toState in trans.Value)
                {
                    if (epsilonTransitions.Contains(new TwoTuple<State<string>, State<string>>(trans.Key.first, toState)))
                    {
                        addTransition(trans.Key.first, trans.Key.second, toState, delta);
                    }
                }
            }

            //add end states
            HashSet<State<string>> allFinalStates = new HashSet<State<string>>();
            allFinalStates.UnionWith(F);
            var epsilonTransitionsToFinalState = new HashSet<TwoTuple<State<string>, State<string>>>(epsilonTransitions.Where(t => t.second.Equals(F.First())));
            foreach (var t in epsilonTransitionsToFinalState)
            {
                allFinalStates.Add(t.first);
            }

            //delete epsilon transitions
            foreach (var pair in delta.Where(t => t.Key.second.Equals("epsilon")))
            {
                delta.Remove(pair.Key);
            }
            alphabet.Remove("epsilon");
            return new NFA<string, string>(states, alphabet, delta, Q_0, allFinalStates);
        }

        public static void addTransition(State<string> from, string letter, State<string> to, Dictionary<TwoTuple<State<string>, string>, HashSet<State<string>>> delta)
        {
            HashSet<State<string>> oldTrans;
            if (delta.TryGetValue(new TwoTuple<State<string>, string>(from, letter), out oldTrans))
            {
                oldTrans.Add(to);
            }
            else
            {
                HashSet<State<string>> set = new HashSet<State<string>>();
                set.Add(to);
                delta.Add(new TwoTuple<State<string>, string>(from, letter), set);
            }
        }

    }

}
