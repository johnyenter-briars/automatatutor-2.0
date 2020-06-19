using AutomataPDL.Automata;
using AutomataPDL.WhileProgram;
using ProblemGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WhileProgramTest
{
    public class Program
    {
        #region test programs
        static WEVar[] x = new WEVar[] { new WEVar(0), new WEVar(1), new WEVar(2) };
        public static WExpr[] programs = new WExpr[]
        {
/*0*/       new WEArith(x[0], x[1], WEArith.ArithOp.minus, 2),
/*1*/       new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1),
/*2*/       new WEConcat(
                new WEArith(x[0], 1, WEArith.ArithOp.plus, 2),
                new WEArith(x[0], 2, WEArith.ArithOp.plus, 1)
                ),
/*3*/       new WEArith(x[0], x[1], WEArith.ArithOp.plus, x[2]),
/*4*/       new WEArith(x[0], 4, WEArith.ArithOp.minus, 4),
/*5*/       new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.leq, 3),
                new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1)),
/*6*/       new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.eq, x[1]),
                new WEArith(x[0], x[0], WEArith.ArithOp.plus, 1)),
/*7*/       new WEConcat(
                new WEArith(x[2], 0, WEArith.ArithOp.plus, 0),
                new WEWhile(
                new WCComparison(x[2], WCComparison.CompareType.l, 4),
                new WEConcat(new WEArith(x[0], x[0], WEArith.ArithOp.plus, x[1]),
                new WEArith(x[2], x[2], WEArith.ArithOp.plus, 1)))),
/*8*/       new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.g, 0),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1)),
/*9*/       new WEIf(
                new WCNot(new WCCompound(new WCComparison(x[0], WCComparison.CompareType.l, 5), WCCompound.Logic.and, new WCComparison(x[0], WCComparison.CompareType.g, 0))),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 0)),
/*10*/      new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.neq, 0),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1)),
/*11*/      new WEArith(x[0], x[0], WEArith.ArithOp.plus, x[0]),
/*12*/      new WEArith(x[0], x[0], WEArith.ArithOp.minus, x[0]),
/*13*/      new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.eq, x[0]),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 0)),
/*14*/      new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.neq, x[0]),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 0)),
/*15*/      new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.leq, x[0]),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 0)),
/*16*/      new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.geq, x[0]),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 0)),
/*17*/      new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.l, x[0]),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 0)),
/*18*/      new WEIf(
                new WCComparison(x[0], WCComparison.CompareType.g, x[0]),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 1),
                new WEArith(x[0], 0, WEArith.ArithOp.plus, 0)),
/*19*/      new WEArith(x[0], 0, WEArith.ArithOp.plus, x[1]),
/*20*/      new WEWhile(new WCComparison(x[0], WCComparison.CompareType.eq, x[0]), new WEArith(x[0], 0)),
/*21*/      new WEIf(
                new WCCompound(
                    new WCComparison(x[0], WCComparison.CompareType.eq, 0),
                    WCCompound.Logic.and,
                    new WCComparison(x[0], WCComparison.CompareType.eq, 0)),
                new WEArith(x[0], 0)),

        };
        #endregion

        private delegate bool programMethod(WExpr program);

        static void Main(string[] args)
        {
            ProgramTest(generate, 21);
            Console.WriteLine("Test complete");
            Console.ReadLine();
        }

        static void ProgramTest(programMethod body, params int[] indices)
        {
            string input;
            int count = 0;

            if (indices.Length == 0)
            {
                foreach (WExpr program in programs)
                {
                    Console.WriteLine($"program #{count}:");
                    ++count;
                    Console.WriteLine(program.ToString());
                    input = Console.ReadLine();
                    if (input == "exit")
                    {
                        return;
                    }
                    if (input == "skip")
                    {
                        continue;
                    }
                    if (input == "enter")
                    {
                        int x = 0;
                    }

                    Console.WriteLine("---------");
                    Console.WriteLine();

                    if (!body(program))
                    {
                        return;
                    }
                    input = Console.ReadLine();
                    if (input == "exit")
                    {
                        return;
                    }
                }
            }
            else
            {
                foreach (int index in indices)
                {
                    WExpr program = programs[index];
                    Console.WriteLine($"program #{count}:");
                    ++count;
                    Console.WriteLine(program.ToString());
                    input = Console.ReadLine();
                    if (input == "exit")
                    {
                        return;
                    }
                    if (input == "skip")
                    {
                        continue;
                    }
                    if (input == "enter")
                    {
                        int x = 0;
                    }

                    Console.WriteLine("---------");
                    Console.WriteLine();

                    if (!body(program))
                    {
                        return;
                    }
                    input = Console.ReadLine();
                    if (input == "exit")
                    {
                        return;
                    }
                }
            }
        }

        static bool uselessVariables(WExpr program)
        {
            foreach (int var in program.GetUselessVariables())
            {
                Console.Write($"{var}, ");
                Console.WriteLine();
            }
            return true;
        }

        static bool toTM(WExpr program)
        {
            TMCB<int, int> M = program.ToTMCB(-1);
            return true;
        }

        //static bool toTMAndRun(WExpr program)
        //{
        //    TMCB<int, int> M = program.ToTMCB(-1);
        //    foreach (int[][] input in WhileUtilities.NonNegIntTestInputs(program.GetNumVariables(), 4, program.GetUselessVariables().ToArray()))
        //    {
        //        Console.WriteLine("input:");
        //        Console.WriteLine(WhileUtilities.PrintTapes(input, M.blank));
        //        bool dummy;
        //        int[][] output = M.Run(input, out dummy);
        //        Console.WriteLine("output:");
        //        Console.WriteLine(WhileUtilities.PrintTapes(output, M.blank));
        //        string s = Console.ReadLine();
        //        if (s == "exit")
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        static bool generate(WExpr program)
        {
            WExpr newProgram;
            ProgramGenerator gen = new ProgramGenerator(program, WEFilter.Filtermode.INFINITE, VariableCache.ConstraintMode.NONE, ProgramBuilder.FilterMode.Trivial);
            while(gen.GenerateWExpr(out newProgram))
            {
                Console.WriteLine(newProgram.ToString());
                string s = Console.ReadLine();
                if(s == "exit")
                {
                    return false;
                }
                if(s == "skip")
                {
                    return true;
                }
            }
            return true;
        }

        static bool numGenerations(WExpr program)
        {
            //ProgramGenerator genNone = new ProgramGenerator(program, WEFilter.Filtermode.NONE, VariableCache.ConstraintMode.NONE, ProgramBuilder.FilterMode.Trivial);
            ProgramGenerator genInputs = new ProgramGenerator(program, WEFilter.Filtermode.INPUTS, VariableCache.ConstraintMode.NONE, ProgramBuilder.FilterMode.Trivial);
            WExpr newprogram;
            //int countNone = 0;
            //while (genNone.GenerateWExpr(out newprogram))
            //{
            //    ++countNone;
            //}
            int countInputs = 0;
            while (genInputs.GenerateWExpr(out newprogram))
            {
                ++countInputs;
            }
            //Console.WriteLine($"None: {countNone}, Inputs: {countInputs}");
            return true;
        }

        static void Generator(int diff)
        {
            WhileToTMGenerator gen = WhileToTMGenerator.GetInstance();
            while (true)
            {
                Console.WriteLine($"difficulty == {diff}");
                WhileToTMProblem prob = gen.Generate(diff);
                Console.WriteLine(prob.whileProgram);
                Console.WriteLine($"diff: {prob.Difficulty()}");
                string s = Console.ReadLine();
                if(s == "exit")
                {
                    return;
                }
                int newDiff;
                if(int.TryParse(s, out newDiff))
                {
                    diff = newDiff;
                    Console.WriteLine($"set difficulty to {diff}");
                }
            }
        }

        static void BasePrograms()
        {
            WhileToTMGenerator gen = WhileToTMGenerator.GetInstance();
            foreach (Tuple<WExpr, int> prog in WhileToTMGenerator.easyBasePrograms)
            {
                Console.WriteLine(prog.Item1);
                Console.ReadKey();
            }
            foreach (Tuple<WExpr, int> prog in WhileToTMGenerator.mediumBasePrograms)
            {
                Console.WriteLine(prog.Item1);
                Console.ReadKey();
            }
            foreach (Tuple<WExpr, int> prog in WhileToTMGenerator.hardBasePrograms)
            {
                Console.WriteLine(prog.Item1);
                Console.ReadKey();
            }
        }

        //old below

        static void Feedback()
        {
            for (int i = 0; i < programs.Length; ++i)
            {
                for (int j = 0; j < programs.Length; ++j)
                {
                    Console.WriteLine($"--- test {i} + {j} ---");

                    var correctProgram = programs[i];
                    var attemptProgram = programs[j];
                    Console.WriteLine(correctProgram.ToString());
                    Console.WriteLine(attemptProgram.ToString());
                    var attemptTm = attemptProgram.ToTMCB(-1);

                    var feedback = AutomataFeedback.FeedbackForWhileToTM(correctProgram, attemptTm, 100);
                    Console.WriteLine(feedback.Item1);
                    foreach (string line in feedback.Item2)
                    {
                        Console.WriteLine(line);
                    }

                    Console.ReadKey();
                }

            }

        }

        static void XML(params int[] indices)
        {
            if (indices.Length == 0)
            {
                foreach (WExpr program in programs)
                {
                    string s1 = program.ToString();
                    XElement xml = new XElement("WhileProgram", new XAttribute("NumVariables", program.GetNumVariables()), program.ToXML());
                    WExpr parsedProgram = WhileUtilities.ParseWhileProgramFromXML(xml);
                    string s2 = parsedProgram.ToString();
                    Console.WriteLine(s1 == s2);
                }
            }
            else
            {
                foreach (int index in indices)
                {
                    WExpr program = programs[index];
                    string s1 = program.ToString();
                    XElement xml = new XElement("WhileProgram", new XAttribute("NumVariables", program.GetNumVariables()), program.ToXML());
                    WExpr parsedProgram = WhileUtilities.ParseWhileProgramFromXML(xml);
                    string s2 = parsedProgram.ToString();
                    Console.WriteLine(s1 == s2);

                }
            }
        }

        static void Generation(params int[] indices)
        {
            if (indices.Length == 0)
            {
                foreach (WExpr program in programs)
                {
                    if (!generationBody(program))
                    {
                        return;
                    }
                }
            }
            else
            {
                foreach (int index in indices)
                {
                    WExpr program = programs[index];
                    if (!generationBody(program))
                    {
                        return;
                    }
                }
            }
        }

        static bool generationBody(WExpr program)
        {
            ProgramGenerator gen = new ProgramGenerator(program, WEFilter.Filtermode.NONE, VariableCache.ConstraintMode.NONE);
            Console.WriteLine("base:");
            Console.WriteLine(program.ToString());
            WExpr newProgram;
            int i = 0;
            string input = "";
            while (true)
            {
                if (gen.GenerateWExpr(out newProgram))
                {
                    Console.WriteLine($"#{i}:");
                    Console.WriteLine(newProgram.ToString());
                    ++i;
                }
                else
                {
                    Console.WriteLine("No more new programs");
                    break;
                }
                input = Console.ReadLine();
                if (input == "exit")
                {
                    return false;
                }
                if (input == "skip")
                {
                    break;
                }
                if (input == "enter")
                {
                    int x = 0;
                }
            }
            return true;
        }

        static void TestStub(params int[] indices)
        {
            if (indices.Length == 0)
            {
                foreach (WExpr program in programs)
                {

                }
            }
            else
            {
                foreach (int index in indices)
                {
                    WExpr program = programs[index];
                }
            }
        }
    }
}
