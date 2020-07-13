using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDA
{
    static internal class PDAXmlParser
    {
        private const string allStackAlphabetSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private static class XmlNames
        {
            internal const string Properties = "properties";
            internal const string AcceptanceCondition = "acceptanceCondition";
            internal const string Determinism = "determinism";
            internal const string StackAlphabet = "stackAlphabet";
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

        internal static PDA<char, char> ParsePDAFromXmlPDA(XElement xmlPDA)
        {
            var properties = xmlPDA.Elements().Where(el => el.Name.LocalName == XmlNames.Properties).First();

            var deterministic = bool.Parse(properties.Elements().Where(el => el.Name.LocalName == XmlNames.Determinism).First().Value);
            var acceptanceCondition = AcceptanceCondition.GetAcceptanceConditionById(properties.Elements().Where(el => el.Name.LocalName == XmlNames.AcceptanceCondition).First().Value);

            var allStackSymbolsAsStrings = properties.Elements().Where(el => el.Name.LocalName == XmlNames.StackAlphabet).First().Elements().Select(s => s.Value).ToList();
            Assertion.Assert(allStackSymbolsAsStrings.All(s => s.Length == 1), "stack symbols must be chars");
            var allStackSymbols = allStackSymbolsAsStrings.Select(s => s.First());
            var firstStackSymbol = allStackSymbols.First();
            //FIXME: what if all keys are used as stack symbols??

            var initialState = xmlPDA.Elements().Where(el => el.Name.LocalName == XmlNames.Nodes).First().Elements().Where(el => el.Name.LocalName == XmlNames.InitialState).First();
            Assertion.Assert(int.Parse(initialState.Attribute(XmlAttr.Id).Value) == PDA<char, char>.initialStateId, "the initial state has to have id " + PDA<char, char>.initialStateId);
            var initialStateIsFinal = bool.Parse(initialState.Attribute(XmlAttr.IsFinal).Value);
            var pda = new PDA<char, char>(acceptanceCondition, deterministic, firstStackSymbol, initialStateIsFinal, allStackSymbols, allStackAlphabetSymbols.First(k => !allStackSymbols.Contains(k)));

            AddStatesToPda(pda, xmlPDA);

            AddTransitionsToPda(pda, xmlPDA);

            return pda;
        }

        private static void AddStatesToPda(PDA<char, char> pda, XElement xmlPDA)
        {
            foreach (var node in xmlPDA.Elements().Where(el => el.Name.LocalName == XmlNames.Nodes).First().Elements().Where(el => el.Name.LocalName == XmlNames.InnerState))
            {
                pda.AddState(int.Parse(node.Attribute(XmlAttr.Id).Value), Boolean.Parse(node.Attribute(XmlAttr.IsFinal).Value));
            }
        }

        private static void AddTransitionsToPda(PDA<char, char> pda, XElement xmlPDA)
        {
            //FIXME: if deterministic is true, but a transition violations the determinism property, then throw a suitable exception, which is catched in the Grader

            foreach (var link in xmlPDA.Elements().Where(el => el.Name.LocalName == XmlNames.Links).First().Elements().Where(el => el.Name.LocalName == XmlNames.Link))
            {
                var start = int.Parse(link.Attribute(XmlAttr.Start).Value);
                var target = int.Parse(link.Attribute(XmlAttr.End).Value);
                foreach (var transition in link.Elements().Where(el => el.Name.LocalName == XmlNames.TransitionGroup).First().Elements().Where(el => el.Name.LocalName == XmlNames.Transition))
                {
                    var inputSymbol = transition.Value[0] == Epsilon ? Symbol<char>.EpsilonIn() : Symbol<char>.SymbolIn(transition.Value[0]);
                    var inputStackSymbol = transition.Value[2];
                    var writtenStackSymbols = transition.Value.Skip(4);
                    pda.AddTransition().From(start).To(target).Read(inputSymbol).Pop(inputStackSymbol).Push(writtenStackSymbols);
                }
            }
        }

        internal static HashSet<char> ParseAlphabetFromXmlPDA(XElement xmlPda)
        {
            return ParseAnAlphabetFromXmlPDA(xmlPda, XmlNames.Alphabet);
        }

        internal static HashSet<char> ParseStackAlphabetFromXmlPDA(XElement xmlPda)
        {
            return ParseAnAlphabetFromXmlPDA(xmlPda, XmlNames.StackAlphabet);
        }

        private static HashSet<char> ParseAnAlphabetFromXmlPDA(XElement xmlPda, string xmlAlphabetName)
        {
            var alphabet = new HashSet<char>();
            var properties = xmlPda.Elements().Where(el => el.Name.LocalName == XmlNames.Properties).First();
            var symbols = properties.Elements().Where(el => el.Name.LocalName == xmlAlphabetName).Elements().Where(el => el.Name.LocalName == XmlNames.Symbol);
            foreach (var symbol in symbols)
            {
                Assertion.Assert(symbol.Value.Length == 1, "alphabet symbols must be chars");
                alphabet.Add(symbol.Value.First());
            }
            return alphabet;
        }
    }
}
