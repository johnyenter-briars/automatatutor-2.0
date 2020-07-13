using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AutomataPDL.CFG;

namespace ProblemGeneration
{
    public class CYKGenerator : Generator<CYKProblem>
    {
        private Random rnd = new Random();
        private static CYKGenerator singleton = null;

        //singleton
        private CYKGenerator() { } //private constructor
        public static CYKGenerator GetInstance()
        {
            if (singleton == null) singleton = new CYKGenerator();
            return singleton;
        }

        override
        public CYKProblem Generate(int targetLevel)
        {
            var grammar = GrammarGenerator.GenerateRandomCNF();

            //generate word
            int wordLength = rnd.Next(4, 6);
            if (targetLevel == Generation.LOW) wordLength = rnd.Next(4, 5); //shorter is easier
            if (targetLevel == Generation.HIGH) wordLength = rnd.Next(5, 6); //longer is harder
            String word = "";
            var terminals = new HashSet<GrammarSymbol>();
            terminals.UnionWith(grammar.GetNonVariableSymbols());
            var terminalsArray = terminals.ToArray();
            if (terminalsArray.Count() == 0)  return new CYKProblem(grammar, word); //no terminals

            //build the word
            for (int i = 0; i < wordLength; i++)
            {
                word += terminalsArray[rnd.Next(terminalsArray.Count())];
            }

            return new CYKProblem(grammar, word);
        }

        public override string TypeName()
        {
            return "CYK Algorithm";
        }
    }

    public class CYKProblem : Problem {

        private ContextFreeGrammar grammar;
        private String word;
        
        Tuple<HashSet<Nonterminal>, List<Tuple<Production, int>>>[][] cykTable; //resulting CYK table

        public CYKProblem(ContextFreeGrammar grammar, string word)
        {
            this.grammar = GrammarUtilities.getEquivalentCNF(grammar);
            this.word = word;

            this.cykTable = GrammarUtilities.cyk(grammar, word);
        }

        //how full is table? (number of NTs per cell)
        private double getFillingDegree()
        {
            int NTCount = 0;
            int CellCount = 0;
            foreach (var x in cykTable)
            {
                foreach (var tuple in x)
                {
                    var NTs = tuple.Item1;
                    NTCount += NTs.Count;
                    CellCount++;
                }
            }
            return (double)NTCount / (double)CellCount;
        }

        //number of cells in table
        private int numberOfCells()
        {
            int n = word.Length;
            return n * (n + 1) / 2;
        }

        //how often do cell values repeat? (1 = never; 0 = very often)
        private double getRepetetiveness()
        {
            var count = new Dictionary<String, int>();
            foreach (var x in cykTable)
            {
                foreach (var tuple in x)
                {
                    var sortedNTs = tuple.Item1.OrderBy(nt => nt.ToString());
                    String s = "";
                    foreach (var nt in sortedNTs) s += nt.ToString() + " ";
                    int currentNumber;
                    if (count.TryGetValue(s, out currentNumber))
                    {
                        count.Remove(s);
                        count.Add(s, currentNumber+1);
                    }
                    else count.Add(s, 1);
                }
            }
            int maxRepeats = 0;
            foreach (var pair in count)
            {
                if (maxRepeats < pair.Value - 1) maxRepeats = pair.Value - 1;
            }

            return 1 - (1.0 * maxRepeats / numberOfCells());

        }

        public override int Difficulty()
        {
            return Generation.Normalization1(10 * word.Length * getFillingDegree());
        }

        public override double Quality()
        {
            if (word.Length <= 0) return 0;

            double res = 1.0;
            double fillingDegree = getFillingDegree();
            //too full
            if (fillingDegree > 2.7) res *= 0.3;
            else if (fillingDegree > 2.3) res *= 0.7;
            //too empty
            if (fillingDegree < 0.2) res *= 0.3;
            else if (fillingDegree < 0.8) res *= 0.7;

            //too short
            if (word.Length <= 1) res *= 0.4;

            //nothing on top
            if (cykTable[cykTable.Length - 1][0].Item1.Count == 0) res *= 0.6;

            //uses useless terminals
            List<char> allowedTerminals = new List<char>();
            foreach(GrammarSymbol s in grammar.GetNonVariableSymbols())
            {
                if (s.ToString().Length >= 1) allowedTerminals.Add(s.ToString()[0]);
            }
            foreach(char c in word)
            {
                if (!allowedTerminals.Contains(c)) res *= 0.6;
            }

            //repetetive
            res *= getRepetetiveness();

            return res;
        }

        override
        public String ToString()
        {
            String s = "";
            foreach (var x in cykTable)
            {
                foreach (var y in x)
                {
                    s += "{";
                    foreach (var nt in y.Item1) s += nt;
                    s += "}";
                }
                s += "\n";
            }
            return s + word + " qual=" + Quality() + " dif=" + Difficulty() + " fill=" + getFillingDegree() + " rep=" + getRepetetiveness() + "\n" + grammar.ToString();
        }

        protected override XElement toXML()
        {
            return new XElement("CYKProblem", 
                    new XElement("Grammar", grammar), 
                    new XElement("Word", word)
                );
        }

        public override Generator GetGenerator()
        {
            return CYKGenerator.GetInstance();
        }
    }
}
