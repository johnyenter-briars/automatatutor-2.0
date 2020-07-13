using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.WhileProgram
{
    public class WhileUtilities
    {
        //returns the least significant bit first binary representation of an integer as a bool[]
        public static bool[] IntToBitsBool(int value)
        {
            string b = Convert.ToString(value, 2);
            bool[] bits = new bool[b.Length];
            for(int i=0; i<b.Length; ++i)
            {
                bits[i] = b[b.Length - 1 - i] == '1';
            }
            return bits;
        }

        //returns the least significant bit first binary representation of an integer as a byte[]
        public static byte[] IntToBitsByte(int value)
        {
            //returns the least significant bit first binary representation of an integer
            //as a byte[]
            string b = Convert.ToString(value, 2);
            byte[] bits = new byte[b.Length];
            for (int i = 0; i < b.Length; ++i)
            {
                Byte.TryParse(""+b[b.Length - 1 - i], out bits[i]);
            }
            return bits;
        }

        //returns the least significant bit first binary representation of an integer as an int[]
        public static int[] IntToBitsInt(int value)
        {
            //returns the least significant bit first binary representation of an integer
            //as an int[]
            string b = Convert.ToString(value, 2);
            int[] bits = new int[b.Length];
            for (int i = 0; i < b.Length; ++i)
            {
                int.TryParse("" + b[b.Length - 1 - i], out bits[i]);
            }
            return bits;
        }

        public static bool TapesEqual(IEnumerable<string> first, IEnumerable<string> second)
        {
            if(first.Count() != second.Count())
            {
                return false;
            }

            var iter1 = first.GetEnumerator();
            var iter2 = second.GetEnumerator();

            while(iter1.MoveNext() && iter2.MoveNext())
            {
                if(iter1.Current != iter2.Current)
                {
                    return false;
                }
            }

            return true;
        }

        /* returns a string representation of <tape>
         * concatenates the string representations of the letters
         * trimms <blank>s on both ends
         * trimms <ignoredLeadingSymbols> at the start
         */
        public static string TapeToString<C>(C[] tape, C blank, C[] ignoredLeadingSymbols, string blankS = null)
        {
            int startIndex = 0;

            for (int i = 0; i < tape.Length; ++i)
            {
                if (!tape[i].Equals(blank))
                {
                    startIndex = i;
                    break;
                }
            }

            if(startIndex >= tape.Length)
            {
                return string.Empty;
            }

            int endIndex = tape.Length - 1;

            for (int i = tape.Length - 1; i >= startIndex; --i)
            {
                if (!tape[i].Equals(blank))
                {
                    endIndex = i;
                    break;
                }
            }

            StringBuilder sb = new StringBuilder();
            bool leadingSymbols = false;
            if(ignoredLeadingSymbols != null)
            {
                leadingSymbols = ignoredLeadingSymbols.Length > 0;
                leadingSymbols = true;
            }
            int firstNonIgnoredIndex = startIndex;

            for (int i = startIndex; i <= endIndex; ++i)
            {
                if (leadingSymbols)
                {
                    if (!ignoredLeadingSymbols.Contains(tape[i]))
                    {
                        firstNonIgnoredIndex = i;
                        leadingSymbols = false;
                    }
                }

                if (!leadingSymbols)
                {
                    if (blankS != null && tape[i].Equals(blank))
                    {
                        sb.Append(blankS);
                    }
                    else
                    {
                        sb.Append(tape[i]);
                    }
                }
            }

            if (leadingSymbols)
            {
                sb.Append(tape[endIndex]);
            }
            else
            {
                if (firstNonIgnoredIndex > 0 && tape[firstNonIgnoredIndex].Equals(blank))
                {
                    sb.Insert(0, tape[firstNonIgnoredIndex - 1]);
                }
            }

            return sb.ToString();
        }

        /* this variant of TapeToString indicates the <headPosition> with a '>'
         * the start/end of the string is only trimmed up to the <headPosition>
         */
        public static string TapeToString<C>(C[] tape, C blank, C[] ignoredLeadingSymbols, int headPosition, string blankS = null)
        {
            int startIndex = 0;

            for (int i = 0; i <= headPosition; ++i)
            {
                if (!tape[i].Equals(blank))
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex >= tape.Length)
            {
                return string.Empty;
            }

            int endIndex = tape.Length - 1;

            for (int i = tape.Length - 1; i >= headPosition; --i)
            {
                if (!tape[i].Equals(blank))
                {
                    endIndex = i;
                    break;
                }
            }

            StringBuilder sb = new StringBuilder();
            bool leadingSymbols = false;
            if (ignoredLeadingSymbols != null)
            {
                leadingSymbols = ignoredLeadingSymbols.Length > 0;
                leadingSymbols = true;
            }
            int firstNonIgnoredIndex = startIndex;

            for (int i = startIndex; i <= endIndex; ++i)
            {
                if (leadingSymbols)
                {
                    if (i >= headPosition || !ignoredLeadingSymbols.Contains(tape[i]))
                    {
                        firstNonIgnoredIndex = i;
                        leadingSymbols = false;
                    }
                }

                if (!leadingSymbols)
                {
                    if(i == headPosition)
                    {
                        sb.Append('>');
                    }
                    if (blankS != null && tape[i].Equals(blank))
                    {
                        sb.Append(blankS);
                    }
                    else
                    {
                        sb.Append(tape[i]);
                    }
                }
            }

            if (firstNonIgnoredIndex > 0 && tape[firstNonIgnoredIndex].Equals(blank))
            {
                sb.Insert(0, tape[firstNonIgnoredIndex - 1]);
            }


            return sb.ToString();
        }

        public static XElement EnumerableToXML<T>(XName name, IEnumerable<T> set)
        {
            XElement rv = new XElement(name);
            foreach (T element in set)
            {
                rv.Add(new XElement("Element", element.ToString()));
            }
            return rv;
        }

        public static IEnumerable<string> ParseEnumerableFromXML(XElement xml)
        {
            foreach(XElement child in xml.Elements())
            {
                yield return child.Value;
            }
        }

        public static WExpr ParseWhileProgramFromXML(XElement program)
        {
            XElement exprs = program.Element("Exprs");
            XElement numVars = program.Element("NumVariables");
            XElement uselessVars = program.Element("UselessVariables");

            int numVariables;
            
            if (int.TryParse(numVars.Value, out numVariables))
            {
                WExprParser parser = new WExprParser(numVariables);
                WExpr rv = parser.parseWExprFromXML((XElement)exprs.FirstNode);
                rv.UselessVariables = new HashSet<int>();
                foreach (string variable in WhileUtilities.ParseEnumerableFromXML(uselessVars))
                {
                    rv.UselessVariables.Add(int.Parse(variable));
                }
                return rv;
            }
            else
            {
                //TODO
                return null;
            }
        }

        public static IEnumerable<int[][]> NonNegIntTestInputs(int numTapes, int inputsPerTape, params int[] fixedTapes)
        {
            //compute permutations in the form of (tapes.Length)-digit numbers with base inputsPerTape

            int[] tapesAsInts = new int[numTapes];
            int[][] tapes = new int[numTapes][];
            for(int i=0; i<numTapes; ++i)
            {
                tapes[i] = new[] { 0 };
            }

            yield return tapes;

            bool exit = false;

            while (true)
            {
                //for each digit
                for (int i = 0; i < numTapes; ++i)
                {
                    //if fixed tape: skip current digit
                    if (fixedTapes.Contains(i))
                    {
                        //if last digit: exit
                        if (i == numTapes - 1)
                        {
                            exit = true;
                        }
                        continue;
                    }

                    //increment digit
                    ++tapesAsInts[i];

                    //if carry
                    if (tapesAsInts[i] == inputsPerTape)
                    {
                        //set digit to 0, update tape
                        tapesAsInts[i] = 0;
                        tapes[i] = IntToBitsInt(0);

                        //if last digit: exit
                        if (i == numTapes - 1)
                        {
                            exit = true;
                        }
                    }
                    //if no carry
                    else
                    {
                        //update tape, break
                        tapes[i] = IntToBitsInt(tapesAsInts[i]);
                        break;
                    }
                }

                if (exit)
                {
                    break;
                }
                else
                {
                    yield return tapes;
                }
            }
        }

        public static WEVar[] VarScope(int size)
        {
            WEVar[] rv = new WEVar[size];
            for(int i=0; i<rv.Length; ++i)
            {
                rv[i] = new WEVar(i);
            }
            return rv;
        }

        public static string RemoveNamespacesFromString(string s)
        {
            while (true)
            {
                int start = s.IndexOf("xmlns");
                if(start == -1)
                {
                    break;
                }
                int first = s.IndexOf('"', start);
                int second = s.IndexOf('"', first + 1);
                int count = second - start + 1;
                s = s.Remove(start, count);
            }
            return s;
        }
    }

    public class WExprParser
    {
        private WEVar[] x;

        public WExprParser(int numVariables)
        {
            this.x = new WEVar[numVariables];
            for(int i=0; i<numVariables; ++i)
            {
                x[i] = new WEVar(i);
            }
        }

        public WExpr parseWExprFromXML(XElement xml)
        {
            //WEVars of program must be well-formed: If a program uses n distinct WEVars, they must have the ids 0...n-1.
            string type = xml.Name.ToString();

            if (type == "var")
            {
                int id = int.Parse(xml.Value);
                return x[id];
            }
            if (type == "const")
            {
                int value = int.Parse(xml.Value);
                return new WEConst(value);
            }
            if (type == "concat")
            {
                XElement[] children = xml.Elements().ToArray();
                WExpr expr1 = parseWExprFromXML(children[0]);
                WExpr expr2 = parseWExprFromXML(children[1]);
                return new WEConcat(expr1, expr2);
            }
            if(type == "while")
            {
                XElement[] children = xml.Elements().ToArray();
                WCond cond = (WCond)parseWExprFromXML(children[0]);
                WExpr body = parseWExprFromXML(children[1]);
                return new WEWhile(cond, body);
            }
            if (type == "if")
            {
                XElement[] children = xml.Elements().ToArray();
                WCond cond = (WCond)parseWExprFromXML(children[0]);
                WExpr thenBody = parseWExprFromXML(children[1]);
                if(children.Count() >= 3)
                {
                    WExpr elseBody = parseWExprFromXML(children[2]);
                    return new WEIf(cond, thenBody, elseBody);
                }
                else
                {
                    return new WEIf(cond, thenBody);
                }
            }
            if (type == "arith")
            {
                WEArith.ArithOp op;
                if (Enum.TryParse(xml.Attribute("op").Value, out op))
                {
                    XElement[] children = xml.Elements().ToArray();
                    WEVar lhs = (WEVar)parseWExprFromXML(children[0]);
                    WEOperand arg1 = (WEOperand)parseWExprFromXML(children[1]);
                    WEOperand arg2 = (WEOperand)parseWExprFromXML(children[2]);
                    return new WEArith(lhs, arg1, op, arg2);
                }
                else
                {
                    //TODO: exception
                    return null;
                }
            }
            if(type == "not")
            {
                WCond cond = (WCond)parseWExprFromXML((XElement)xml.FirstNode);
                return new WCNot(cond);
            }
            if (type == "compare")
            {
                WCComparison.CompareType op;
                if (Enum.TryParse(xml.Attribute("op").Value, out op))
                {
                    XElement[] children = xml.Elements().ToArray();
                    WEVar arg1 = (WEVar)parseWExprFromXML(children[0]);
                    WEOperand arg2 = (WEOperand)parseWExprFromXML(children[1]);
                    return new WCComparison(arg1, op, arg2);
                }
                else
                {
                    //TODO: exception
                    return null;
                }
            }
            if(type == "compound")
            {
                WCCompound.Logic op;
                if(Enum.TryParse(xml.Attribute("op").Value, out op))
                {
                    XElement[] children = xml.Elements().ToArray();
                    WCond cond1 = (WCond)parseWExprFromXML(children[0]);
                    WCond cond2 = (WCond)parseWExprFromXML(children[1]);
                    return new WCCompound(cond1, op, cond2);
                }
                else
                {
                    //TODO: exception
                    return null;
                }

            }
            //TODO: exception
            return null;
        }

    }

    /* class used for generating WExprs from CWExprs
    * - ensures well-formed variable ids (contiguous from 0 to some n)
    * - ensures that there are no duplicate WEVars and WEConsts
    * - optionally uses filters to prevent unwanted programs
    */
    public class ProgramBuilder
    {
        private Dictionary<int, WEVar> varDict;
        private Dictionary<int, WEConst> constDict;
        private List<BuilderFilter> filters;
        private bool keepProgram;
        public enum FilterMode { Trivial }

        public ProgramBuilder(params FilterMode[] filterModes)
        {
            this.varDict = new Dictionary<int, WEVar>();
            this.constDict = new Dictionary<int, WEConst>();
            this.filters = new List<BuilderFilter>();
            this.keepProgram = true;
            if (filterModes.Contains(FilterMode.Trivial))
            {
                this.filters.Add(new TrivialFilter());
            }
        }

        public WEVar NewVar(int id)
        {
            WEVar rv;
            if (!varDict.TryGetValue(id, out rv))
            {
                rv = new WEVar(id);
                varDict.Add(id, rv);
            }
            return rv;
        }

        public WEConst NewConst(int value)
        {
            WEConst rv;
            if (!constDict.TryGetValue(value, out rv))
            {
                rv = new WEConst(value);
                constDict.Add(value, rv);
            }
            return rv;
        }

        public void CheckExpr(WExpr expr)
        {
            if (this.keepProgram)
            {
                foreach (BuilderFilter filter in filters)
                {
                    if (!filter.KeepProgram(expr))
                    {
                        this.keepProgram = false;
                        return;
                    }
                }
            }
            return;
        }

        public void FinalizeProgram()
        {
            int index = 0;
            
            foreach(KeyValuePair<int, WEVar> variable in varDict)
            {
                variable.Value.id = index;
                ++index;
            }
        }

        public bool KeepProgram()
        {
            return keepProgram;
        }
    }

    //abstract super class for filters used by ProgramBuilder
    public abstract class BuilderFilter
    {
        public abstract bool KeepProgram(WExpr expr);
    }

    public class TrivialFilter : BuilderFilter
    {
        public override bool KeepProgram(WExpr expr)
        {
            #region filter out: x_i = x_i
            if(expr is WEArith)
            {
                WEArith e = (WEArith)expr;
                if(e.op == WEArith.ArithOp.plus || e.op == WEArith.ArithOp.minus)
                {
                    if (e.arg1 is WEVar && e.arg2 is WEConst)
                    {
                        if (e.lhs.id == ((WEVar)e.arg1).id && ((WEConst)e.arg2).value == 0)
                        {
                            return false;
                        }
                    }
                    if (e.arg2 is WEVar && e.arg1 is WEConst)
                    {
                        if (e.lhs.id == ((WEVar)e.arg2).id && ((WEConst)e.arg1).value == 0)
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion

            #region filter out: x = x_i - x_i
            if(expr is WEArith)
            {
                WEArith e = (WEArith)expr;
                if(e.op == WEArith.ArithOp.minus)
                {
                    if(e.arg1 is WEVar && e.arg2 is WEVar)
                    {
                        if(((WEVar)e.arg1).id == ((WEVar)e.arg2).id)
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion

            #region filter out: unsatisfiable and tautological comparisons
            if (expr is WCComparison)
            {
                WCComparison e = (WCComparison)expr;
                if(e.arg2 is WEVar)
                {
                    if(e.arg1.id == ((WEVar)e.arg2).id)
                    {
                        return false;
                    }
                }
                else
                {
                    if (e.arg2.IsZero() && e.op == WCComparison.CompareType.l)
                    {
                        return false;
                    }
                }
            }
            #endregion

            #region filter out: if statements where the <then> and <else> parts are the same
            if(expr is WEIf)
            {
                WEIf e = (WEIf)expr;
                if(e.elseBody != null)
                {
                    if(e.thenBody.ToString() == e.elseBody.ToString())
                    {
                        return false;
                    }
                }
            }
            #endregion

            #region filter out: compound conditions of the form C_i op C_i
            if(expr is WCCompound)
            {
                WCCompound e = (WCCompound)expr;
                if(e.cond1.ToString() == e.cond2.ToString())
                {
                    return false;
                }
            }
            #endregion

            return true;
        }
    }
}
