using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AutomataPDL.CFG;

namespace ProblemGeneration
{
    public class WordsInGrammarGenerator : Generator<WordsInGrammarProblem>
    {
        private Random rnd = new Random();
        private static WordsInGrammarGenerator singleton = null;

        //singleton
        private WordsInGrammarGenerator() { } //private constructor
        public static WordsInGrammarGenerator GetInstance()
        {
            if (singleton == null) singleton = new WordsInGrammarGenerator();
            return singleton;
        }

        override
        public WordsInGrammarProblem Generate(int targetLevel)
        {
            ContextFreeGrammar grammar = null;
            var inNeeded = 1;
            var outNeeded = 1;          
            // We iterate until we get a CNF
            grammar = GrammarGenerator.GenerateRandom();
            while (GrammarUtilities.getEquivalentCNF(grammar) == null)
            {
                grammar = GrammarGenerator.GenerateRandom();
            }

            inNeeded = rnd.Next(1, 5);
            outNeeded = rnd.Next(1, 5);
            if (targetLevel == Generation.HIGH)
            {
                inNeeded = rnd.Next(3, 5);
                outNeeded = rnd.Next(3, 5);
            }
            else if (targetLevel == Generation.MEDIUM)
            {
                inNeeded = rnd.Next(2, 4);
                outNeeded = rnd.Next(2, 4);
            }
            else if (targetLevel == Generation.LOW)
            {
                inNeeded = rnd.Next(1, 4);
                outNeeded = rnd.Next(1, 4);
            }
            return new WordsInGrammarProblem(grammar, inNeeded, outNeeded);
        }

        public override string TypeName()
        {
            return "Grammar Words";
        }
    }

    public class WordsInGrammarProblem : Problem {

        private ContextFreeGrammar grammar;
        private int inNeeded;
        private int outNeeded;

        private int numberOfShortWords; //number of words with length <= 4
        private int possibleShortWords; //number of words with length <= 4
        private static readonly int shortLength = 4;

        public WordsInGrammarProblem(ContextFreeGrammar grammar, int inNeeded, int outNeeded)
        {
            this.grammar = grammar;
            this.inNeeded = inNeeded;
            this.outNeeded = outNeeded;
            
            numberOfShortWords = GrammarUtilities.generateShortestWords(grammar, shortLength).Count;
            //calculate all possible short words (needed for qual and dif)
            possibleShortWords = 0;
            int termCount = grammar.GetNonVariableSymbols().Count();
            int x = 1;
            for (int i = 0; i <= shortLength; i++)
            {
                possibleShortWords += x;
                x *= termCount;
            }
        }

        public override int Difficulty()
        {
            // grammar size (20%)
            double grammarDif = 20.0;
            if (grammar.GetProductions().Count() < 8) grammarDif *= 0.7; //productions
            else if (grammar.GetProductions().Count() < 6) grammarDif *= 0.5;
            else if (grammar.GetProductions().Count() < 4) grammarDif *= 0.1;
            if (grammar.Variables.Count() < 4) grammarDif *= 0.5; //nonterminals
            else if (grammar.Variables.Count() < 3) grammarDif *= 0.1;

            // amount of work (50%)
            double numberOfNeededWordsDif = 50.0 / 3 * (inNeeded-1) / 3 * (outNeeded-1);

            // hard to find solutions (30%)
            double sparseDif = 30.0;
            double inPerc = numberOfShortWords * 1.0 / possibleShortWords;
            double outPerc = 1 - inPerc;
            double sparsity = Math.Min(inPerc, outPerc);
            if (sparsity >= 0.3) sparseDif *= 0;
            else if (sparsity >= 0.2) sparseDif *= 0.2;
            else if (sparsity >= 0.1) sparseDif *= 0.5;
            else if (sparsity >= 0.05) sparseDif *= 0.8;
            

            return (int)Math.Round(grammarDif + numberOfNeededWordsDif + sparseDif);
        }

        public override double Quality()
        {
            //not enough words in grammar
            if (numberOfShortWords < inNeeded) return 0;

            //not enough words NOT in grammar
            if (possibleShortWords - numberOfShortWords < outNeeded) return 0;

            //NTs not usefull
            return grammar.GetUsefulNonterminals().Count * 1.0 / grammar.Variables.Count;
        }

        // Test for generated Grammars
        public override bool isValid()
        {
            // Important! Set the length of words to be tested
            var testWordsUpToLength = 5;
            if (GrammarUtilities.getEquivalentCNF(this.grammar) == null)
                return false;
            Tuple<int, int> tup = GrammarUtilities.getMinWordInAndOut(GrammarUtilities.getEquivalentCNF(this.grammar), testWordsUpToLength, this.inNeeded, this.outNeeded);
            return tup.Item1 >= this.inNeeded && tup.Item2 >= this.outNeeded;
        }

        override
        public String ToString()
        {
            double inPerc = numberOfShortWords * 1.0 / possibleShortWords;
            double outPerc = 1 - inPerc;
            double sparsity = Math.Min(inPerc, outPerc);
            return " qual=" + Quality() + " dif=" + Difficulty() + " inNeeded=" + inNeeded + " outNeeded=" + outNeeded + " inPerc=" + inPerc + " outPerc=" + outPerc + " sparcity=" + sparsity + "\n" + grammar.ToString();
        }

        protected override XElement toXML()
        {
            return new XElement("WordsInGrammarProblem",
                    new XElement("Grammar", grammar),
                    new XElement("InNeeded", inNeeded),
                    new XElement("OutNeeded", outNeeded)
                );
        }

        public override Generator GetGenerator()
        {
            return WordsInGrammarGenerator.GetInstance();
        }
    }
}
