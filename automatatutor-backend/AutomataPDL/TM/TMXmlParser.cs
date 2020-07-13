using AutomataPDL.Automata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.TM
{
    public static class TMXmlParser
    {
        private static class XmlNames
        {
            internal const string Properties = "properties";
            internal const string Alphabet = "alphabet";
            internal const string Symbol = "symbol";

            internal const string Nodes = "nodes";
            internal const string InitialState = "initialState";
            internal const string InnerState = "innerState";
            internal const string Links = "links";
            internal const string Link = "link";
            internal const string TransitionGroup = "transitionGroup";
            internal const string Transition = "transition";
        }

        private static class XmlAttr
        {
            internal const string IsFinal = "isFinal";
            internal const string Id = "id";
            internal const string Start = "start";
            internal const string End = "end";
        }

        private const char Epsilon = 'E';

        public static TMCB<int, string> ParseTMFromXml(XElement xmlTM)
        {
            var QandF = GetStates(xmlTM);
            var Q = QandF.Item1;
            var F = QandF.Item2;

            int blank = -1;

            var initialStateXml = xmlTM.Elements().Where(el => el.Name.LocalName == XmlNames.Nodes).First().Elements().Where(el => el.Name.LocalName == XmlNames.InitialState).First();
            int initialStateId = int.Parse(initialStateXml.Attribute(XmlAttr.Id).Value);
            var initialState = new State<string>(initialStateId, initialStateId.ToString());
            Q.Add(initialState);
            if (bool.Parse(initialStateXml.Attribute(XmlAttr.IsFinal).Value))
            {
                F.Add(initialState);
            }

            var idStateDict = new Dictionary<int, State<string>>();
            foreach (var q in Q)
            {
                idStateDict.Add(q.id, q);
            }

            var gammaAndSigma = ParseGammaAndSigma(xmlTM, blank);
            var gamma = gammaAndSigma.Item1;
            var sigma = gammaAndSigma.Item2;

            var tm = new TMCB<int, string>(Q, F, sigma, gamma, blank);

            tm.q0 = initialState;

            AddTransitionsToTM(xmlTM, tm, idStateDict);

            return tm;
        }

        private static int ParseSymbol(char symbol)
        {
            switch (symbol)
            {
                case '0': return 0;
                case '1': return 1;
                case 'E': return -1;
                default: throw new Exception("invalid symbol: " + symbol);
            }
        }

        private static void AddTransitionsToTM(XElement xmlTm, TMCB<int, string> tm, Dictionary<int, State<string>> idStateDict)
        {
            foreach (var link in xmlTm.Elements().Where(el => el.Name.LocalName == XmlNames.Links).First().Elements().Where(el => el.Name.LocalName == XmlNames.Link))
            {
                var start = int.Parse(link.Attribute(XmlAttr.Start).Value);
                var target = int.Parse(link.Attribute(XmlAttr.End).Value);
                var startState = idStateDict[start];
                var targetState = idStateDict[target];

                foreach (var transitionXmlElement in link.Elements().Where(el => el.Name.LocalName == XmlNames.TransitionGroup).First().Elements().Where(el => el.Name.LocalName == XmlNames.Transition))
                {
                    var transitionsForTapes = transitionXmlElement.Value.Split('|');
                    var newTransition = new Transition<int, string>();
                    var i = 0;
                    foreach(var t in transitionsForTapes)
                    {
                        var inputSymbol = ParseSymbol(t[0]);
                        var outputSymbol = ParseSymbol(t[2]);
                        var moveSymbol = t[4];

                        newTransition.AddReadCondition(i, inputSymbol);
                        if (inputSymbol != outputSymbol)
                        {
                            newTransition.AddWriteAction(i, outputSymbol);
                        }
                        if (moveSymbol == 'R')
                        {
                            newTransition.AddMoveAction(i, Directions.Right);
                        }
                        if (moveSymbol == 'L')
                        {
                            newTransition.AddMoveAction(i, Directions.Left);
                        }

                        i++;
                    }


                    if (startState != targetState)
                    {
                        newTransition.TargetState = targetState;
                    }

                    tm.AddTransition(startState, newTransition);
                }
            }
        }

        private static Tuple<HashSet<int>, HashSet<int>> ParseGammaAndSigma(XElement xmlTM, int blank)
        {
            var properties = xmlTM.Elements().Where(el => el.Name.LocalName == XmlNames.Properties).First();
            var allAlphabetSymbolsAsStrings = properties.Elements().Where(el => el.Name.LocalName == XmlNames.Alphabet).First().Elements().Select(s => s.Value).ToList();

            if (!allAlphabetSymbolsAsStrings.All(symbol => symbol.Equals("0") || symbol.Equals("1")))
            {
                throw new Exception("Only 0 and 1 are allowed as alphabet symbols");
            }

            var sigma = new HashSet<int>(new int[] { 0, 1 });
            var gamma = new HashSet<int>(new int[] { 0, 1, blank });

            return new Tuple<HashSet<int>, HashSet<int>>(gamma, sigma);
        }

        private static Tuple<HashSet<State<string>>, HashSet<State<string>>> GetStates(XElement xmlTM)
        {
            var Q = new HashSet<State<string>>();
            var F = new HashSet<State<string>>();

            foreach (var node in xmlTM.Elements().Where(el => el.Name.LocalName == XmlNames.Nodes).First().Elements().Where(el => el.Name.LocalName == XmlNames.InnerState))
            {
                int id = int.Parse(node.Attribute(XmlAttr.Id).Value);
                var state = new State<string>(id, id.ToString());
                Q.Add(state);

                if (bool.Parse(node.Attribute(XmlAttr.IsFinal).Value))
                {
                    F.Add(state);
                }
            }
            return new Tuple<HashSet<State<string>>, HashSet<State<string>>>(Q, F);
        }
    }
}
