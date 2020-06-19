using AutomataPDL.Automata;
using AutomataPDL.WhileProgram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.WhileProgram
{
    public class Program
    {
        #region test programs
        static WEVar[] x = new WEVar[] { new WEVar(0), new WEVar(1), new WEVar(2) };
        public static WExpr[] programs = new WExpr[]
        {
/*0*/       new WEArith(x[0], x[1], WEArith.ArithOp.minus, 3),
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
/*11*/      new WEArith(x[0], x[0], WEArith.ArithOp.plus, x[0])
        };
        #endregion

        private delegate bool programMethod(WExpr program);

        static void Main(string[] args)
        {
            ProgramTest(toTM, 11);
            Console.WriteLine("Test complete");
            Console.ReadKey();
        }

        static void ProgramTest(programMethod body, params int[] indices)
        {
            string input;

            if (indices.Length == 0)
            {
                foreach (WExpr program in programs)
                {
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

                    if (!body(program))
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

                    if (!body(program))
                    {
                        return;
                    }
                }
            }
        }

        static bool uselessVariables(WExpr program)
        {
            foreach(int var in program.GetUselessVariables())
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

        static void ConversionAndTMFunctionality(params int[] indices)
        {
            if(indices.Length == 0)
            {
                foreach(WExpr program in programs)
                {
                    Console.WriteLine(program.ToString());
                    Console.ReadKey();
                    var M = program.ToTMCB(-1);
                    foreach(int[][] input in WhileUtilities.NonNegIntTestInputs(program.GetNumVariables(), 4, program.GetUselessVariables().ToArray()))
                    {
                        Console.WriteLine("input:");
                        Console.WriteLine(WhileUtilities.TapesToString(input, M.blank));
                        bool dummy;
                        int[][] output = M.Run(input, out dummy);
                        Console.WriteLine("output:");
                        Console.WriteLine(WhileUtilities.TapesToString(output, M.blank));
                        Console.ReadKey();
                    }
                }
            }
            else
            {
                foreach(int index in indices)
                {
                    Console.WriteLine(programs[index].ToString());
                    Console.ReadKey();
                    var M = programs[index].ToTMCB(-1);
                    foreach (int[][] input in WhileUtilities.NonNegIntTestInputs(programs[index].GetNumVariables(), 4, programs[index].GetUselessVariables().ToArray()))
                    {
                        Console.WriteLine("input:");
                        Console.WriteLine(WhileUtilities.TapesToString(input, M.blank));
                        bool dummy;
                        int[][] output = M.Run(input, out dummy);
                        Console.WriteLine("output:");
                        Console.WriteLine(WhileUtilities.TapesToString(output, M.blank));
                        Console.ReadKey();
                    }

                }
            }
        }

        static void NullTapes()
        {
            foreach (WExpr program in programs)
            {
                Console.WriteLine(program.ToString());
                Console.ReadKey();
                var M = program.ToTMCB(-1);
                bool dummy;
                int[][] output = M.Run(program.GetNumVariables(), out dummy);
                Console.WriteLine("output:");
                Console.WriteLine(WhileUtilities.TapesToString(output, M.blank));
                Console.ReadKey();
            }

        }

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
                if(input == "enter")
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
