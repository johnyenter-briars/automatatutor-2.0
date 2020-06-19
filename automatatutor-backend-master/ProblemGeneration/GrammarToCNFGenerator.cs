using System;
using System.Linq;
using System.Xml.Linq;
using AutomataPDL.CFG;

namespace ProblemGeneration
{
    public class GrammarToCNFGenerator : Generator<GrammarToCNFProblem>
    {
        private Random rnd = new Random();
        private static GrammarToCNFGenerator singleton = null;

        //singleton
        private GrammarToCNFGenerator() { } //private constructor
        public static GrammarToCNFGenerator GetInstance()
        {
            if (singleton == null) singleton = new GrammarToCNFGenerator();
            return singleton;
        }

        override
        public GrammarToCNFProblem Generate(int targetLevel)
        {
            var grammar = GrammarGenerator.GenerateRandom();
            return new GrammarToCNFProblem(grammar);
        }

        public override string TypeName()
        {
            return "Chomsky Normalform";
        }
    }

    public class GrammarToCNFProblem : Problem {

        private ContextFreeGrammar grammar;

        private ContextFreeGrammar sol;

        public GrammarToCNFProblem(ContextFreeGrammar grammar)
        {
            this.grammar = grammar;

            sol = GrammarUtilities.getEquivalentCNF(grammar);
        }

        public override int Difficulty()
        {
            int inputProd = grammar.GetProductions().Count();

            int outputProd = 0;
            if (sol != null) outputProd = sol.GetProductions().Count();

            // input grammar size (20%)
            double inputGrammarDif = 20.0 * Math.Min(inputProd, 10) / 10;

            // output grammar size (80%)
            double outputGrammarDif = 80.0 * Math.Min(outputProd,15) / 15;


            return (int)Math.Round(inputGrammarDif + outputGrammarDif);
        }

        public override double Quality()
        {
            if (sol == null) return 0;
            
            double res = 1.0;
            if (sol.GetProductions().Count() >= 15) res *= 0.8;
            if (sol.GetProductions().Count() >= 20) res *= 0.6;
            if (sol.GetProductions().Count() >= 25) res *= 0.3;
            if (sol.GetProductions().Count() >= 30) res *= 0.1;

            //NTs not usefull
            res *= grammar.GetUsefulNonterminals().Count * 1.0 / grammar.Variables.Count;

            return res;
        }

        override
        public String ToString()
        {
            int inputProd = grammar.GetProductions().Count();
            int outputProd = 0;
            if (sol != null) outputProd = sol.GetProductions().Count();
            return " qual=" + Quality() + " dif=" + Difficulty() + " inputProd=" + inputProd + " outputProd=" + outputProd + "\n" + grammar.ToString() + "\n ------- \n" + sol;
        }

        protected override XElement toXML()
        {
            return new XElement("GrammarToCNFProblem",
                    new XElement("Grammar", grammar)
                );
        }

        public override Generator GetGenerator()
        {
            return GrammarToCNFGenerator.GetInstance();
        }
    }
}
