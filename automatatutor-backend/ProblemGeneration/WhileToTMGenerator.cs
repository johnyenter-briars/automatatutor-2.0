using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutomataPDL.WhileProgram;
using AutomataPDL.Automata;

namespace ProblemGeneration
{
    public class WhileToTMGenerator : Generator<WhileToTMProblem>
    {
        private static WhileToTMGenerator singleton = null;

        //base programs: new problems are generated from these programs, resulting in similar difficulty.

        #region base programs
        private static WEVar[] x = WhileUtilities.VarScope(3);

        public static Tuple<WExpr, int>[] easyBasePrograms =
        {
            //if (x == 0) then x++
            //diff 10
            new Tuple<WExpr, int>(new WEIf(new WCComparison(x[0], WCComparison.CompareType.eq, 0), new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1)), 10),
            //x++
            //diff 10
            new Tuple<WExpr, int>(new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1), 20),
            //x = y; x++
            //diff 30
            new Tuple<WExpr, int>(new WEConcat(new WEArith(x[0], x[1]), new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1)), 30),
        };
        public static Tuple<WExpr, int>[] mediumBasePrograms =
        {
            //if (x == 0) then x++ else y = x
            //diff 40
            new Tuple<WExpr, int>(new WEIf(new WCComparison(x[0], WCComparison.CompareType.eq, 0), new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1), new WEArith(x[1], x[0])), 40),
            //if (x == 0 && y == 0) then x++ else y++
            //diff 50
            new Tuple<WExpr, int>(
                new WEIf(
                    new WCCompound(
                        new WCComparison(x[0], WCComparison.CompareType.eq, 0),
                        WCCompound.Logic.and,
                        new WCComparison(x[1], WCComparison.CompareType.eq, 0)),
                    new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1),
                    new WEArith(x[1], x[1], WEArith.ArithOp.plus, 1)),
                50)
        };
        public static Tuple<WExpr, int>[] hardBasePrograms =
        {
            //x = x + y
            //diff 70
            new Tuple<WExpr, int>(new WEArith(x[0], x[0], WEArith.ArithOp.plus, x[1]), 70),
            //if (x > y) then x++
            //diff 80
            new Tuple<WExpr, int>(new WEIf(new WCComparison(x[0], WCComparison.CompareType.g, x[1]), new WEArith(x[0], x[0], WEArith.ArithOp.plus, x[1])), 80),
            //while (x != 0) do y = y + y; x-- end
            //diff 90
            new Tuple<WExpr, int>(new WEWhile(new WCComparison(x[0], WCComparison.CompareType.neq, 0), new WEConcat(new WEArith(x[1], x[1], WEArith.ArithOp.plus, x[1]), new WEArith(x[0], x[0], WEArith.ArithOp.minus, 1))), 100),
            //while (x < y) do y = y - x end
            //diff 100
            new Tuple<WExpr, int>(
                new WEWhile(
                    new WCComparison(x[0], WCComparison.CompareType.l, x[1]),
                    new WEArith(x[1], x[1], WEArith.ArithOp.minus,x[0])),                        
                100),
        };
        #endregion

        private static ProgramGenerator[] easyGenerators = new ProgramGenerator[easyBasePrograms.Length];
        private static ProgramGenerator[] mediumGenerators = new ProgramGenerator[mediumBasePrograms.Length];
        private static ProgramGenerator[] hardGenerators = new ProgramGenerator[hardBasePrograms.Length];

        private static VariableCache.ConstraintMode constMode = VariableCache.ConstraintMode.NONE;
        private static WEFilter.Filtermode filterMode = WEFilter.Filtermode.INFINITE;
        private static ProgramBuilder.FilterMode[] builderFilterModes = new[] { ProgramBuilder.FilterMode.Trivial };

        private WhileToTMGenerator() { } //private constructor
        public static WhileToTMGenerator GetInstance()
        {
            if (singleton == null) singleton = new WhileToTMGenerator();
            return singleton;
        }

        private ProgramGenerator getGenerator(int programIndex, int difficulty)
        {
            Tuple<WExpr, int>[] programArray = null;
            ProgramGenerator[] genArray = null;
            if (difficulty == Generation.LOW)
            {
                programArray = easyBasePrograms;
                genArray = easyGenerators;
            }
            if (difficulty == Generation.MEDIUM)
            {
                programArray = mediumBasePrograms;
                genArray = mediumGenerators;
            }
            if (difficulty == Generation.HIGH)
            {
                programArray = hardBasePrograms;
                genArray = hardGenerators;
            }
            if(genArray == null || programArray == null)
            {
                return null;
            }
            else
            {
                if (genArray[programIndex] == null)
                {
                    genArray[programIndex] = new ProgramGenerator(programArray[programIndex].Item1, filterMode, constMode, builderFilterModes);
                }
                return genArray[programIndex];
            }
        }

        public override WhileToTMProblem Generate(int targetDifficulty)
        {
            Random rd = new Random();
            ProgramGenerator tmGenerator;
            Tuple<WExpr, int>[] programArray = easyBasePrograms;

            #region set tmGenerator and programArray according to difficulty
            if (targetDifficulty == Generation.ANY)
            {
                targetDifficulty = rd.Next(0, 3);
            }
            if(targetDifficulty == 0)
            {
                targetDifficulty = Generation.LOW;
            }
            if (targetDifficulty == 1)
            {
                targetDifficulty = Generation.MEDIUM;
                programArray = mediumBasePrograms;
            }
            if (targetDifficulty == 2)
            {
                targetDifficulty = Generation.HIGH;
                programArray = hardBasePrograms;
            }
            

            int index = rd.Next(0, programArray.Length);
            tmGenerator = getGenerator(index, targetDifficulty);
            #endregion

            int resultDifficulty = programArray[index].Item2;

            WExpr generatedProgram;
            if(!tmGenerator.GenerateWExpr(out generatedProgram))
            {
                tmGenerator.Reset();
                tmGenerator.GenerateWExpr(out generatedProgram);
            }
            return new WhileToTMProblem(generatedProgram, resultDifficulty);
        }

        public override string TypeName()
        {
            return "While to TM";
        }
    }

    public class WhileToTMProblem : Problem
    {
        public WExpr whileProgram;
        private int difficulty;

        //private static XElement binaryAlphabet = new XElement("Alphabet", new XElement("symbol", 0), new XElement("symbol", 1), new XElement("symbol", '\u25FB'));

        public WhileToTMProblem(WExpr whileProgram, int difficulty)
        {
            this.whileProgram = whileProgram;
            this.difficulty = difficulty;
        }

        //TODO
        public override int Difficulty()
        {
            return difficulty;
        }

        public override Generator GetGenerator()
        {
            return WhileToTMGenerator.GetInstance();
        }

        //TODO
        public override double Quality()
        {
            return 1;
        }

        protected override XElement toXML()
        {
            XElement rv = new XElement("WhileToTMProblem",
                whileProgram.ProgramToXML(),
                new XElement("ProgramText", whileProgram.ToString())
                );
            return rv;
        }
    }
}
