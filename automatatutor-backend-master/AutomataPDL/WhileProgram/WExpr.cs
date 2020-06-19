using AutomataPDL.Automata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.WhileProgram
{
    /* base class for while program expressions
     * 
     * syntax:
     * P -> P P
     *   |  X = V ( + | - ) V
     *   |  if C then P ( else P ) ?
     *   |  while C do P
     * C -> not C
     *   |  X ( == | < | <= | != | > | >= ) V
     *   |  C ( && | || ) C
     * X -> x_0 | x_1 | ...
     * V -> X
     *   |  0 | 1 | ...
     */
    public abstract class WExpr
    {
        public HashSet<int> Variables;
        public HashSet<int> UselessVariables;

        public int GetNumVariables()
        {
            if (Variables == null)
            {
                Variables = new HashSet<int>();
                HashSet<int> usefulVariables = new HashSet<int>();
                CollectVariables(Variables, usefulVariables, false);
                UselessVariables = new HashSet<int>(Variables);
                UselessVariables.ExceptWith(usefulVariables);
            }
            return Variables.Count;
        }

        public IEnumerable<int> GetUselessVariables()
        {
            if (UselessVariables == null)
            {
                GetNumVariables();
            }
            return UselessVariables;
        }

        public abstract void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful);

        public abstract CWExpr GetCWE();

        public string ToString(string prefix)
        {
            return prefix + ToString().Replace($"{Environment.NewLine}", $"{Environment.NewLine}{prefix}");
        }

        public XElement ProgramToXML()
        {
            return new XElement("Program",
                new XElement("Exprs", ToXML()),
                new XElement("NumVariables", GetNumVariables()),
                WhileUtilities.EnumerableToXML("UselessVariables", GetUselessVariables()),
                new XElement("UselessVariablesText", string.Join(", ", GetUselessVariables()))
                );
        }

        public abstract XElement ToXML();

        public abstract override string ToString();

        public TMCB<int, int> ToTMCB(int blank)
        {
            TMCB<int, int> M = new TMCB<int, int>();
            M.Sigma.Add(0);
            M.Sigma.Add(1);
            M.Gamma = new HashSet<int>(M.Sigma);
            M.Gamma.Add(blank);
            M.blank = blank;
            M.q0 = M.CreateState(0);
            State<int> last = M.CreateState(1);
            M.Q.Add(last);
            M.F.Add(last);

            this.BuildTMCB(M.q0, last, M);
            return M;
        }

        public abstract void BuildTMCB(State<int> pred, State<int> succ, TMCB<int, int> M);

        protected static void addGoToEnd(Directions dir, TMCB<int, int> M, State<int> pred, State<int> succ, params int[] tapes)
        {
            //Only pass multiple tapes if you know they will the specified end simultaneously.
            if (dir == Directions.Non)
            {
                return;
            }
            else
            {
                //default: move all tapes in direction dir
                Transition<int, int> t = new Transition<int, int>();
                foreach(int tape in tapes)
                {
                    t.AddMoveAction(tape, dir);
                }
                M.AddDefaultTransition(pred, t);
                //if all blanks: move all tapes in direction !dir, -> succ
                t = new Transition<int, int>();
                foreach (int tape in tapes)
                {
                    t.AddReadCondition(tape, M.blank);
                    t.AddMoveAction(tape, dir == Directions.Left ? Directions.Right : Directions.Left);
                }
                t.TargetState = succ;
                M.AddTransition(pred, t);
            }
        }

        protected static void addCopyTape(TMCB<int, int> M, State<int> pred, State<int> succ, int source, params int[] targets)
        {
            //assumes M.Gamma == {-1, 0, 1}, M.blank == -1
            //auto trims and rewinds
            Transition<int, int> t;
            State<int> p = M.CreateState(M.Q.Count);
            State<int> q;

            //if source 0 or 1: copy letter, R
            foreach (int letter in new[] { 0, 1 })
            {
                t = new Transition<int, int>();
                t.AddReadCondition(source, letter);
                t.AddMoveAction(source, Directions.Right);
                foreach(int tape in targets)
                {
                    t.AddWriteAction(tape, letter);
                    t.AddMoveAction(tape, Directions.Right);
                }
                M.AddTransition(pred, t);
            }
            //source reads blank: source L, -> p
            t = new Transition<int, int>();
            t.AddReadCondition(source, M.blank);
            t.AddMoveAction(source, Directions.Left);
            t.TargetState = p;
            M.AddTransition(pred, t);

            //trim all targets (auto-rewinds)
            foreach(int tape in targets)
            {
                q = M.CreateState(M.Q.Count);
                addTrimTape(M, p, q, tape);
                p = q;
            }

            //rewind source
            addGoToEnd(Directions.Left, M, p, succ, source);
        }

        protected static void addTrimTape(TMCB<int, int> M, State<int> pred, State<int> succ, int tape)
        {
            //deletes all non-blank letters from current position to right end
            //and deletes all leading 0s
            //if tape == 0...0, it becomes 0
            //then goes to left end
            State<int> q = M.CreateState(M.Q.Count);
            State<int> r = M.CreateState(M.Q.Count);
            Transition<int, int> t;

            #region pred
            //default: write 0, R
            t = new Transition<int, int>();
            t.AddWriteAction(tape, 0);
            t.AddMoveAction(tape, Directions.Right);
            M.AddDefaultTransition(pred, t);
            //if blank: L, -> q
            t = new Transition<int, int>();
            t.AddReadCondition(tape, M.blank);
            t.AddMoveAction(tape, Directions.Left);
            t.TargetState = q;
            M.AddTransition(pred, t);
            #endregion

            #region q
            //if 0: write blank, L
            t = new Transition<int, int>();
            t.AddReadCondition(tape, 0);
            t.AddWriteAction(tape, M.blank);
            t.AddMoveAction(tape, Directions.Left);
            M.AddTransition(q, t);
            //if blank: write 0, -> succ
            t = new Transition<int, int>();
            t.AddReadCondition(tape, M.blank);
            t.AddWriteAction(tape, 0);
            t.TargetState = succ;
            M.AddTransition(q, t);
            //if 1: -> r
            t = new Transition<int, int>();
            t.AddReadCondition(tape, 1);
            t.TargetState = r;
            M.AddTransition(q, t);
            #endregion

            addGoToEnd(Directions.Left, M, r, succ, tape);
        }

    }

    public abstract class WEOperand : WExpr
    {
        public abstract bool IsZero();

        public override void BuildTMCB(State<int> pred, State<int> succ, TMCB<int, int> M)
        {
            //do nothing :)
        }
    }

    public class WEVar : WEOperand
    {
        public int id;

        public WEVar(int id)
        {
            this.id = id;
        }

        public override bool IsZero()
        {
            return false;
        }

        public override CWExpr GetCWE()
        {
            return new CWEVar(id);
        }

        public override string ToString()
        {
            return $"x_{id}";
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            vars.Add(id);
            if (useful)
            {
                usefulVars.Add(id);
            }
        }

        public override XElement ToXML()
        {
            return new XElement("var", id);
        }

    }

    public class WEConst : WEOperand
    {
        public int value;

        public WEConst(int value)
        {
            this.value = Math.Max(0, value);
        }

        public override bool IsZero()
        {
            return value == 0;
        }

        public override CWExpr GetCWE()
        {
            return new CWEConst(value);
        }

        public override string ToString()
        {
            return $"{value}";
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            //do nothing
        }

        public override XElement ToXML()
        {
            return new XElement("const", value);
        }

    }

    public class WEConcat : WExpr
    {
        public WExpr expr1;
        public WExpr expr2;

        public WEConcat(WExpr expr1, WExpr expr2)
        {
            this.expr1 = expr1;
            this.expr2 = expr2;
        }

        public static WEConcat BuildProgram(IEnumerable<WExpr> expressions)
        {
            IEnumerator<WExpr> currentExpr = expressions.GetEnumerator();
            //if expressions has <2 elements, return null
            currentExpr.MoveNext();
            if (currentExpr.MoveNext())
            {
                currentExpr.Reset();
                currentExpr.MoveNext();
                return (WEConcat)recBuildProgram(currentExpr);
            }
            else return null;
        }

        private static WExpr recBuildProgram(IEnumerator<WExpr> currentExpr)
        {
            WExpr expr1 = currentExpr.Current;
            if (currentExpr.MoveNext())
            {
                return new WEConcat(expr1, recBuildProgram(currentExpr));
            }
            else
            {
                return expr1;
            }
        }

        public override void BuildTMCB(State<int> pred, State<int> succ, TMCB<int, int> M)
        {
            State<int> q = M.CreateState(M.Q.Count);
            expr1.BuildTMCB(pred, q, M);
            expr2.BuildTMCB(q, succ, M);
        }

        public override CWExpr GetCWE()
        {
            return new CWEConcat(expr1.GetCWE(), expr2.GetCWE());
        }

        public override string ToString()
        {
            return $"{expr1.ToString()}{Environment.NewLine}{expr2.ToString()}";
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            expr1.CollectVariables(vars, usefulVars, false);
            expr2.CollectVariables(vars, usefulVars, false);
        }

        public override XElement ToXML()
        {
            return new XElement("concat", expr1.ToXML(), expr2.ToXML());
        }

    }

    public class WEWhile : WExpr
    {
        public WCond condition;
        public WExpr body;

        public WEWhile(WCond condition, WExpr body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override CWExpr GetCWE()
        {
            return new CWEWhile((CWCond)condition.GetCWE(), body.GetCWE());
        }

        public override string ToString()
        {
            string condStr = condition.ToString();
            string bodyStr = body.ToString("\t");
            return $"while {condStr} do{Environment.NewLine}{bodyStr}{Environment.NewLine}endwhile";
        }

        public override void BuildTMCB(State<int> pred, State<int> succ, TMCB<int, int> M)
        {
            State<int> q = M.CreateState(M.Q.Count);
            condition.BuildTMCB(pred, q, succ, M);
            body.BuildTMCB(q, pred, M);
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            condition.CollectVariables(vars, usefulVars, false);
            body.CollectVariables(vars, usefulVars, false);
        }

        public override XElement ToXML()
        {
            return new XElement("while", condition.ToXML(), body.ToXML());
        }

    }

    public class WEIf : WExpr
    {
        public WCond condition;
        public WExpr thenBody;
        public WExpr elseBody;

        public WEIf(WCond condition, WExpr thenBody, WExpr elseBody)
        {
            this.condition = condition;
            this.thenBody = thenBody;
            this.elseBody = elseBody;
        }

        public WEIf(WCond condition, WExpr thenBody) : this(condition, thenBody, null)
        {

        }

        public override CWExpr GetCWE()
        {
            return new CWEIf((CWCond)condition.GetCWE(), thenBody.GetCWE(), (elseBody != null) ? elseBody.GetCWE() : null);
        }

        public override string ToString()
        {
            string condStr = condition.ToString();
            string thenStr = thenBody.ToString("\t");

            if(elseBody != null)
            {
                string elseStr = elseBody.ToString("\t");
                return $"if {condStr} then{Environment.NewLine}{thenStr}{Environment.NewLine}else{Environment.NewLine}{elseStr}{Environment.NewLine}endif";
            }
            else
            {
                return $"if {condStr} then{Environment.NewLine}{thenStr}{Environment.NewLine}endif";
            }
        }

        public override void BuildTMCB(State<int> pred, State<int> succ, TMCB<int, int> M)
        {
            State<int> q = M.CreateState(M.Q.Count);
            State<int> p = M.CreateState(M.Q.Count);
            if(elseBody != null)
            {
                condition.BuildTMCB(pred, p, q, M);
                thenBody.BuildTMCB(p, succ, M);
                elseBody.BuildTMCB(q, succ, M);
            }
            else
            {
                condition.BuildTMCB(pred, p, succ, M);
                thenBody.BuildTMCB(p, succ, M);
            }
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            condition.CollectVariables(vars, usefulVars, false);
            thenBody.CollectVariables(vars, usefulVars, false);
            if(elseBody != null)
            {
                elseBody.CollectVariables(vars, usefulVars, false);
            }
        }

        public override XElement ToXML()
        {
            if(elseBody != null)
            {
                return new XElement("if", condition.ToXML(), thenBody.ToXML(), elseBody.ToXML());
            }
            else
            {
                return new XElement("if", condition.ToXML(), thenBody.ToXML());
            }
        }

    }

    public class WEArith : WExpr
    {
        public enum ArithOp { plus, minus }

        public readonly WEVar lhs;
        public readonly WEOperand arg1;
        public readonly ArithOp op;
        public readonly WEOperand arg2;

        public WEArith(WEVar lhs, WEOperand arg1, ArithOp op, WEOperand arg2)
        {
            this.lhs = lhs;
            this.arg1 = arg1;
            this.op = op;
            this.arg2 = arg2;
        }

        public WEArith(WEVar lhs, int c1, ArithOp op, WEOperand arg2) : this(lhs, new WEConst(c1), op, arg2)
        {

        }

        public WEArith(WEVar lhs, WEOperand arg1, ArithOp op, int c2) : this(lhs, arg1, op, new WEConst(c2))
        {

        }

        public WEArith(WEVar lhs, int c1, ArithOp op, int c2) : this(lhs, new WEConst(c1 + c2), op, new WEConst(0))
        {

        }

        public WEArith(WEVar lhs, WEOperand assign) : this(lhs, assign, ArithOp.plus, new WEConst(0))
        {

        }

        public WEArith(WEVar lhs, int assign) : this(lhs, new WEConst(assign), ArithOp.plus, new WEConst(0))
        {

        }

        public override CWExpr GetCWE()
        {
            CWEVar lhsChoice = (CWEVar)lhs.GetCWE();
            CWEOperand arg1Choice;
            CWEOperand arg2Choice;

            if (op == ArithOp.minus || op == ArithOp.plus)
            {
                //case: x = y or x = c: -> CWEAssign
                if (arg1.IsZero())
                {
                    arg2Choice = (CWEOperand)arg2.GetCWE();
                    return new CWEAssign(lhsChoice, arg2Choice);
                }
                if (arg2.IsZero())
                {
                    arg1Choice = (CWEOperand)arg1.GetCWE();
                    return new CWEAssign(lhsChoice, arg1Choice);
                }

                //case: x = x + 1 or x = x - 1 -> CWEIncDec
                WEVar x;
                WEConst c;
                if(arg1 is WEVar && arg2 is WEConst)
                {
                    x = (WEVar)arg1;
                    c = (WEConst)arg2;
                    if (x.id == lhs.id && c.value == 1)
                    {
                        return new CWEIncDec((CWEVar)lhs.GetCWE());
                    }
                }
                if (arg2 is WEVar && arg1 is WEConst)
                {
                    x = (WEVar)arg2;
                    c = (WEConst)arg1;
                    if (x.id == lhs.id && c.value == 1)
                    {
                        return new CWEIncDec((CWEVar)lhs.GetCWE());
                    }
                }
            }

            //case: standard arithmetic operation
            arg1Choice = (CWEOperand)arg1.GetCWE();
            arg2Choice = (CWEOperand)arg2.GetCWE();

            switch (this.op)
            {
                case ArithOp.plus:
                    return new CWEArithSimple(lhsChoice, arg1Choice, arg2Choice);
                case ArithOp.minus:
                    return new CWEArithSimple(lhsChoice, arg1Choice, arg2Choice);
            }

            return null;
        }

        public override string ToString()
        {
            string opStr = "";

            if(op == ArithOp.minus || op == ArithOp.plus)
            {
                if (arg1.IsZero())
                {
                    return $"{lhs} = {arg2}";
                }
                if (arg2.IsZero())
                {
                    return $"{lhs} = {arg1}";
                }
            }

            switch (op)
            {
                case ArithOp.plus: opStr = "+"; break;
                case ArithOp.minus: opStr = "-"; break;
            }
            return $"{lhs} = {arg1} {opStr} {arg2}";
        }

        public override void BuildTMCB(State<int> pred, State<int> succ, TMCB<int, int> M)
        {
            //assumes M.Gamma == {-1, 0, 1}, M.blank == -1;
            int[] letters = { -1, 0, 1 };
            int x = lhs.id;
            int y, z;
            State<int> step1, step2, step3, p, q, carry, normal;
            State<int> writeZero, trim;
            byte[] bits;

            Transition<int, int> s, t;
            
            #region case 1: 2 variables

            if (arg1 is WEVar && arg2 is WEVar)
            {
                y = ((WEVar)arg1).id;
                z = ((WEVar)arg2).id;

                step1 = M.CreateState(M.Q.Count);
                step2 = M.CreateState(M.Q.Count);

                if (x != y)
                {
                    if (x != z)
                    {
                        addCopyTape(M, pred, step1, y, x);
                        pred = step1;
                    }
                    else
                    {
                        int tmp = y;
                        y = z;
                        z = tmp;
                    }
                }

                switch (op)
                {
                    #region Plus
                    case ArithOp.plus:

                        //if all tapes distinct (x = y + z):
                        //copy y -> x
                        //this converts the problem to x += z
                        //
                        //if exactly one tape is distinct from x:
                        //swap y and z if necessary so that z is the distinct tape
                        //
                        //if all tapes are the same:
                        //x = x + x, equivalent to shift right

                        carry = M.CreateState(M.Q.Count);

                        //x += z

                        //addition
                        //when z reaches blank, it stops moving until the end of this step
                        #region pred / carry
                        if(x == z)
                        {
                            t = new Transition<int, int>();
                            t.AddReadCondition(x, 0);
                            t.AddMoveAction(x, Directions.Right);
                            M.AddTransition(pred, t);

                            t = new Transition<int, int>();
                            t.AddReadCondition(x, 1);
                            t.AddWriteAction(x, 0);
                            t.TargetState = carry;
                            t.AddMoveAction(x, Directions.Right);
                            M.AddTransition(pred, t);

                            t = new Transition<int, int>();
                            t.AddReadCondition(x, M.blank);
                            t.TargetState = step2;

                            s = new Transition<int, int>();
                            s.AddReadCondition(x, 0);
                            s.AddWriteAction(x, 1);
                            s.TargetState = pred;
                            s.AddMoveAction(x, Directions.Right);
                            M.AddTransition(carry, s);

                            s = new Transition<int, int>();
                            s.AddReadCondition(x, 1);
                            s.AddMoveAction(x, Directions.Right);
                            M.AddTransition(carry, s);

                            s = new Transition<int, int>();
                            s.AddReadCondition(x, M.blank);
                            s.AddWriteAction(x, 1);
                            s.TargetState = step2;
                            s.AddMoveAction(x, Directions.Right);
                            M.AddTransition(carry, s);

                        }
                        else
                        {
                            foreach (int letter1 in letters)
                            {
                                foreach (int letter2 in letters)
                                {
                                    t = new Transition<int, int>();
                                    s = new Transition<int, int>();
                                    t.AddReadCondition(x, letter1);
                                    t.AddReadCondition(z, letter2);
                                    s.AddReadCondition(x, letter1);
                                    s.AddReadCondition(z, letter2);

                                    //if both blanks:
                                    //normal: x N, z L, -> step2
                                    //carry: write 1, x R, z L, -> step2
                                    if (letter1 == -1 && letter2 == -1)
                                    {
                                        t.TargetState = step2;
                                        s.TargetState = step2;
                                        s.AddWriteAction(x, 1);
                                        s.AddMoveAction(x, Directions.Right);
                                        if (x != z)
                                        {
                                            t.AddMoveAction(z, Directions.Left);
                                            s.AddMoveAction(z, Directions.Left);
                                        }
                                    }
                                    //otherwise: compute sum (blank ^= 0), carrySum == sum + 1
                                    else
                                    {
                                        int sum = Math.Max(0, letter1) + Math.Max(0, letter2);
                                        int carrySum = sum + 1;

                                        //x R
                                        //z R unless blank
                                        t.AddMoveAction(x, Directions.Right);
                                        s.AddMoveAction(x, Directions.Right);
                                        if (letter2 != -1)
                                        {
                                            t.AddMoveAction(z, Directions.Right);
                                            s.AddMoveAction(z, Directions.Right);
                                        }

                                        //sum <= 1: write sum
                                        //sum > 1: write sum % 2, -> carry
                                        if (sum > 1)
                                        {
                                            sum = sum % 2;
                                            t.TargetState = carry;
                                        }
                                        t.AddWriteAction(x, sum);

                                        //carrySum <= 1: write carrySum, -> pred
                                        //carrySum > 1: write carrySum % 2
                                        if (carrySum > 1)
                                        {
                                            carrySum = carrySum % 2;
                                        }
                                        else
                                        {
                                            s.TargetState = pred;
                                        }
                                        s.AddWriteAction(x, carrySum);
                                    }
                                    M.AddTransition(pred, t);
                                    M.AddTransition(carry, s);
                                }
                            }
                        }
                        #endregion

                        //trim
                        //if x != z: rewind z 
                        if (x != z)
                        {
                            step3 = M.CreateState(M.Q.Count);
                            addTrimTape(M, step2, step3, x);
                            addGoToEnd(Directions.Left, M, step3, succ, z);
                        }
                        else
                        {
                            addTrimTape(M, step2, succ, x);
                        }
                        break;
                    #endregion

                    #region Minus
                    //if result < 0: returns 0 instead.
                    case ArithOp.minus:

                        #region bitwise difference
                        //pred / carry
                        carry = M.CreateState(M.Q.Count);
                        writeZero = M.CreateState(M.Q.Count);
                        trim = M.CreateState(M.Q.Count);

                        if (x == z)
                        {
                            //if all tapes are the same, write 0 to x
                            addTrimTape(M, pred, succ, x);
                            return;
                        }
                        foreach (int letter1 in letters)
                        {
                            foreach (int letter2 in letters)
                            {
                                s = new Transition<int, int>();
                                t = new Transition<int, int>();

                                s.AddReadCondition(x, letter1);
                                s.AddReadCondition(z, letter2);
                                t.AddReadCondition(x, letter1);
                                t.AddReadCondition(z, letter2);

                                if (letter1 == M.blank && letter2 == M.blank)
                                {
                                    //normal: z L, -> trim
                                    s.AddMoveAction(z, Directions.Left);
                                    s.TargetState = trim;

                                    //carry: x L, z L, -> writeZero
                                    t.AddMoveAction(x, Directions.Left);
                                    t.AddMoveAction(z, Directions.Left);
                                    t.TargetState = writeZero;
                                }
                                if (letter1 == M.blank && letter2 != M.blank)
                                {
                                    //x L, -> writeZero
                                    s.AddMoveAction(x, Directions.Left);
                                    s.TargetState = writeZero;

                                    t.AddMoveAction(x, Directions.Left);
                                    t.TargetState = writeZero;
                                }
                                if (letter1 != M.blank && letter2 == M.blank)
                                {
                                    //normal: x R, z N
                                    s.AddMoveAction(x, Directions.Right);

                                    //carry: z N, standard difference
                                    int carryDiff = letter1 - 1;
                                    if (carryDiff < 0)
                                    {
                                        carryDiff = 2 + carryDiff;
                                    }
                                    else
                                    {
                                        t.TargetState = pred;
                                    }
                                    t.AddMoveAction(x, Directions.Right);
                                    t.AddWriteAction(x, carryDiff);
                                }
                                if (letter1 != M.blank && letter2 != M.blank)
                                {
                                    //standard difference
                                    int diff = letter1 - letter2;
                                    int carryDiff = diff - 1;

                                    s.AddMoveAction(x, Directions.Right);
                                    s.AddMoveAction(z, Directions.Right);
                                    if (diff < 0)
                                    {
                                        diff = 2 + diff;
                                        s.TargetState = carry;
                                    }
                                    s.AddWriteAction(x, diff);

                                    t.AddMoveAction(x, Directions.Right);
                                    t.AddMoveAction(z, Directions.Right);
                                    if (carryDiff < 0)
                                    {
                                        carryDiff = 2 + carryDiff;
                                    }
                                    else
                                    {
                                        t.TargetState = pred;
                                    }
                                    t.AddWriteAction(x, carryDiff);
                                }

                                M.AddTransition(pred, s);
                                M.AddTransition(carry, t);
                            }
                        }

                        #endregion

                        #region trim / rewind tapes
                        State<int> rewindz = M.CreateState(M.Q.Count);
                        addTrimTape(M, trim, rewindz, x);
                        addGoToEnd(Directions.Left, M, rewindz, succ, z);
                        #endregion

                        #region write zero
                        //delete everything right to left
                        //then write one 0
                        t = new Transition<int, int>();
                        t.AddMoveAction(x, Directions.Left);
                        t.AddWriteAction(x, M.blank);
                        M.AddDefaultTransition(writeZero, t);

                        t = new Transition<int, int>();
                        t.AddReadCondition(x, M.blank);
                        t.AddWriteAction(x, 0);
                        t.TargetState = rewindz;
                        M.AddTransition(writeZero, t);
                        #endregion

                        break;
                    #endregion

                }
                return;
            }
            #endregion

            #region case 2: 2 constants

            if (arg1 is WEConst && arg2 is WEConst)
            {
                int c1 = ((WEConst)arg1).value;
                int c2 = ((WEConst)arg2).value;

                int result;

                switch (op)
                {
                    #region Plus
                    case ArithOp.plus:
                        result = c1 + c2;
                        bits = WhileUtilities.IntToBitsByte(result);
                        p = pred;

                        #region states p_i
                        for (int i = 0; i < bits.Length; ++i)
                        {
                            //always: write bit, move right
                            t = new Transition<int, int>();
                            t.AddWriteAction(x, bits[i]);
                            t.AddMoveAction(x, Directions.Right);
                            q = M.CreateState(M.Q.Count);
                            t.TargetState = q;
                            M.AddTransition(p, t);
                            p = q;
                        }
                        #endregion

                        addTrimTape(M, p, succ, x);

                        break;
                    #endregion

                    #region Minus
                    case ArithOp.minus:

                        result = Math.Max(0, c1 - c2);
                        bits = WhileUtilities.IntToBitsByte(result);

                        #region bitwise write result
                        p = pred;
                        for (int i = 0; i < bits.Length; ++i)
                        {
                            //always: write bit, move right
                            t = new Transition<int, int>();
                            t.AddWriteAction(x, bits[i]);
                            t.AddMoveAction(x, Directions.Right);
                            q = M.CreateState(M.Q.Count);
                            t.TargetState = q;
                            M.AddTransition(p, t);
                            p = q;
                        }
                        #endregion

                        #region trim
                        addTrimTape(M, p, succ, x);
                        #endregion

                        break;
                        #endregion
                }
                return;
            }
            #endregion

            #region case 3: 1 variable + 1 constant
            int c;
            if(arg1 is WEVar)
            {
                y = ((WEVar)arg1).id;
                c = ((WEConst)arg2).value;
            }
            else
            {
                y = ((WEVar)arg2).id;
                c = ((WEConst)arg1).value;
            }
            bits = WhileUtilities.IntToBitsByte(c);

            //if x != y: copy y -> x
            if (x != y)
            {
                step1 = M.CreateState(M.Q.Count);
                addCopyTape(M, pred, step1, y, x);
                pred = step1;
            }

            switch (op)
            {
                #region Plus
                case ArithOp.plus:

                    //x += c

                    normal = M.CreateState(M.Q.Count);
                    carry = M.CreateState(M.Q.Count);

                    #region pred
                    foreach (int letter in letters)
                    {
                        int sum = Math.Max(0, letter) + bits[0];
                        t = new Transition<int, int>();
                        t.AddReadCondition(x, letter);
                        t.AddMoveAction(x, Directions.Right);
                        if (sum <= 1)
                        {
                            t.TargetState = normal;
                        }
                        else
                        {
                            sum = sum % 2;
                            t.TargetState = carry;
                        }
                        t.AddWriteAction(x, sum);
                        M.AddTransition(pred, t);
                    }
                    #endregion

                    #region p,q -> normal, carry

                    p = normal;
                    q = carry;

                    for (int i = 1; i < bits.Length; ++i)
                    {
                        normal = M.CreateState(M.Q.Count);
                        carry = M.CreateState(M.Q.Count);

                        foreach (int letter in letters)
                        {
                            int sum = Math.Max(0, letter) + bits[i];
                            int carrySum = sum + 1;
                            s = new Transition<int, int>();
                            s.AddReadCondition(x, letter);
                            s.AddMoveAction(x, Directions.Right);
                            if (sum > 1)
                            {
                                sum = sum % 2;
                                s.TargetState = carry;
                            }
                            s.AddWriteAction(x, sum);
                            M.AddTransition(p, s);

                            t = new Transition<int, int>();
                            t.AddReadCondition(x, letter);
                            t.AddMoveAction(x, Directions.Right);
                            if (carrySum > 1)
                            {
                                carrySum = carrySum % 2;
                            }
                            else
                            {
                                t.TargetState = normal;
                            }
                            t.AddWriteAction(x, carrySum);
                            M.AddTransition(q, t);
                        }

                        p = normal;
                        q = carry;
                    }
                    #endregion

                    #region last p,q

                    step1 = M.CreateState(M.Q.Count);

                    //p: L, -> step1
                    t = new Transition<int, int>();
                    t.AddMoveAction(x, Directions.Left);
                    t.TargetState = step1;
                    M.AddDefaultTransition(p, t);

                    //q: write sum
                    //if carry: stay in q
                    //else: L, -> step1
                    foreach (int letter in letters)
                    {
                        int sum = Math.Max(0, letter) + 1;
                        t = new Transition<int, int>();
                        t.AddReadCondition(x, letter);
                        if (sum > 1)
                        {
                            sum = sum % 2;
                            t.AddMoveAction(x, Directions.Right);
                        }
                        else
                        {
                            t.AddMoveAction(x, Directions.Left);
                            t.TargetState = step1;
                        }
                        t.AddWriteAction(x, sum);
                        M.AddTransition(q, t);
                    }
                    #endregion

                    //step1: rewind x
                    addGoToEnd(Directions.Left, M, step1, succ, x);

                    break;
                #endregion

                #region Minus
                case ArithOp.minus:

                    #region bitwise difference
                    writeZero = M.CreateState(M.Q.Count);
                    trim = M.CreateState(M.Q.Count);
                    State<int> rewind = M.CreateState(M.Q.Count);
                    p = pred;
                    q = M.CreateState(M.Q.Count);

                    for(int i=0; i<bits.Length; ++i)
                    {
                        normal = M.CreateState(M.Q.Count);
                        carry = M.CreateState(M.Q.Count);

                        foreach(int letter in letters)
                        {
                            s = new Transition<int, int>();
                            t = new Transition<int, int>();
                            s.AddReadCondition(x, letter);
                            t.AddReadCondition(x, letter);

                            if(letter == M.blank)
                            {
                                s.TargetState = writeZero;
                                s.AddMoveAction(x, Directions.Left);
                                t.TargetState = writeZero;
                                t.AddMoveAction(x, Directions.Left);
                            }
                            else
                            {
                                int diff = letter - bits[i];
                                int carryDiff = diff - 1;

                                if(diff < 0)
                                {
                                    diff = 2 + diff;
                                    s.TargetState = carry;
                                }
                                else
                                {
                                    s.TargetState = normal;
                                }
                                s.AddWriteAction(x, diff);
                                s.AddMoveAction(x, Directions.Right);

                                if (carryDiff < 0)
                                {
                                    carryDiff = 2 + carryDiff;
                                    t.TargetState = carry;
                                }
                                else
                                {
                                    t.TargetState = normal;
                                }
                                t.AddWriteAction(x, carryDiff);
                                t.AddMoveAction(x, Directions.Right);
                            }

                            M.AddTransition(p, s);
                            M.AddTransition(q, t);
                        }

                        p = normal;
                        q = carry;
                    }
                    #endregion

                    #region after last bit
                    //as long as there still is a carry to subtract, perform bitwise subtraction
                    //if end of tape is reached: -> trim if no carry, -> writeZero if carry
                    //carry is gone before end of tape is reached: -> rewind
                    foreach(int letter in letters)
                    {
                        s = new Transition<int, int>();
                        t = new Transition<int, int>();
                        s.AddReadCondition(x, letter);
                        t.AddReadCondition(x, letter);

                        if(letter == M.blank)
                        {
                            s.TargetState = trim;
                            t.TargetState = writeZero;
                            t.AddMoveAction(x, Directions.Left);
                        }
                        else
                        {
                            s.TargetState = rewind;

                            int carryDiff = letter - 1;
                            if(carryDiff < 0)
                            {
                                carryDiff = 2 + carryDiff;
                            }
                            else
                            {
                                t.TargetState = p;
                            }
                            t.AddWriteAction(x, carryDiff);
                            t.AddMoveAction(x, Directions.Right);
                        }
                        M.AddTransition(p, s);
                        M.AddTransition(q, t);
                    }
                    #endregion

                    #region rewind
                    addGoToEnd(Directions.Left, M, rewind, succ, x);
                    #endregion

                    #region trim
                    addTrimTape(M, trim, succ, x);
                    #endregion

                    #region writeZero
                    //only reached when x == blank
                    //delete everything right to left
                    //then write one 0
                    t = new Transition<int, int>();
                    t.AddMoveAction(x, Directions.Left);
                    t.AddWriteAction(x, M.blank);
                    M.AddDefaultTransition(writeZero, t);

                    t = new Transition<int, int>();
                    t.AddReadCondition(x, M.blank);
                    t.AddWriteAction(x, 0);
                    t.TargetState = succ;
                    M.AddTransition(writeZero, t);

                    #endregion

                    break;
                    #endregion
            }

            #endregion
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            lhs.CollectVariables(vars, usefulVars, false);
            arg1.CollectVariables(vars, usefulVars, true);
            arg2.CollectVariables(vars, usefulVars, true);
        }

        public override XElement ToXML()
        {

            return new XElement("arith", new XAttribute("op", op.ToString()), lhs.ToXML(), arg1.ToXML(), arg2.ToXML());
        }

    }

    public abstract class WCond : WExpr
    {
        public override void BuildTMCB(State<int> pred, State<int> succ, TMCB<int, int> M)
        {
            BuildTMCB(pred, succ, succ, M);
        }

        public abstract void BuildTMCB(State<int> pred, State<int> succTrue, State<int> succFalse, TMCB<int, int> M);

    }

    public class WCNot : WCond
    {
        public WCond condition;

        public WCNot(WCond condition)
        {
            this.condition = condition;
        }

        public override void BuildTMCB(State<int> pred, State<int> succTrue, State<int> succFalse, TMCB<int, int> M)
        {
            condition.BuildTMCB(pred, succFalse, succTrue, M);
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            condition.CollectVariables(vars, usefulVars, false);
        }

        public override CWExpr GetCWE()
        {
            return condition.GetCWE();
        }

        public override string ToString()
        {
            return $"!({condition.ToString()})";
        }

        public override XElement ToXML()
        {
            return new XElement("not", condition.ToXML());
        }
    }

    public class WCComparison : WCond
    {
        public enum CompareType { eq, l, leq, neq, g, geq }

        public WEVar arg1;
        public CompareType op;
        public WEOperand arg2;

        public WCComparison(WEVar arg1, CompareType op, WEOperand arg2)
        {
            this.arg1 = arg1;
            this.op = op;
            this.arg2 = arg2;
        }

        public WCComparison(WEVar arg1, CompareType op, int constant)
        {
            this.arg1 = arg1;
            this.op = op;
            this.arg2 = new WEConst(constant);
        }

        public override CWExpr GetCWE()
        {
            CWEVar arg1Choice = (CWEVar)arg1.GetCWE();

            if((op==CompareType.eq || op==CompareType.neq) && arg2.IsZero())
            {
                return new CWCSimple(arg1Choice);
            }
            else
            {
                CWEOperand arg2Choice = (CWEOperand)arg2.GetCWE();
                return new CWCComplex(arg1Choice, arg2Choice);
            }
        }

        public override string ToString()
        {
            string opStr = "";

            switch (op)
            {
                case CompareType.eq: opStr = "=="; break;
                case CompareType.l: opStr = "<"; break;
                case CompareType.leq: opStr = "<="; break;
                case CompareType.neq: opStr = "!="; break;
                case CompareType.g: opStr = ">"; break;
                case CompareType.geq: opStr = ">="; break;
            }

            return $"{arg1.ToString()} {opStr} {arg2.ToString()}";
        }

        public override void BuildTMCB(State<int> pred, State<int> succTrue_in, State<int> succFalse_in, TMCB<int, int> M)
        {
            State<int> accept = M.CreateState(M.Q.Count);
            State<int> reject = M.CreateState(M.Q.Count);
            Transition<int, int> t;
            State<int> p, q, r, s, presumeTrue, presumeFalse;
            int tape1 = arg1.id;
            State<int> succTrue = succTrue_in;
            State<int> succFalse = succFalse_in;

            //if 2 variables and tape1 == tape2:
            //comparison is always true or always false. Go to succTrue, succFalse directly.
            if(arg2 is WEVar)
            {
                int tape2 = ((WEVar)arg2).id;

                if (tape1 == tape2)
                {
                    t = new Transition<int, int>();
                    if (op == CompareType.g || op == CompareType.l || op == CompareType.neq)
                    {
                        t.TargetState = succFalse;
                    }
                    else
                    {
                        t.TargetState = succTrue;
                    }
                    M.AddDefaultTransition(pred, t);
                    return;
                }
            }

            //convert neq, g, geq -> eq, l, leq by switching succTrue and succFalse
            if (op == CompareType.neq || op == CompareType.g || op == CompareType.geq)
            {
                State<int> temp = succTrue;
                succTrue = succFalse;
                succFalse = temp;
            }

            #region arg2 is a variable
            //tapes are always moved synchronously
            //so they can be rewound simultaneously at the end
            if (arg2 is WEVar)
            {
                int tape2 = ((WEVar)arg2).id;

                if(op == CompareType.eq || op == CompareType.neq)
                {
                    #region Equals
                    #region state pred
                    //compare tapes left to right
                    //reject at first difference
                    //accept if right end is reached
                    //for each tape: stops on last non-blank symbol if it exists. Otherwise on a blank, one left of start position.

                    //accept if blank/blank is read
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddReadCondition(tape2, M.blank);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = accept;
                    M.AddTransition(pred, t);

                    //move right if same letter is read (except blank)
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, 0);
                    t.AddReadCondition(tape2, 0);
                    t.AddMoveAction(tape1, Directions.Right);
                    t.AddMoveAction(tape2, Directions.Right);
                    M.AddTransition(pred, t);
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, 1);
                    t.AddReadCondition(tape2, 1);
                    t.AddMoveAction(tape1, Directions.Right);
                    t.AddMoveAction(tape2, Directions.Right);
                    M.AddTransition(pred, t);

                    //default: reject, move left
                    t = new Transition<int, int>();
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = reject;
                    M.AddDefaultTransition(pred, t);
                    #endregion

                    #endregion
                }

                if (op == CompareType.l || op == CompareType.geq)
                {
                    #region Less Than
                    //almost identical to LEQ

                    #region state pred
                    //move right until the end of at least one tape is reached.
                    //accept if tape1 is shorter.
                    //reject if tape2 is shorter.
                    //if tapes have the same length: -> q
                    //for each tape: stops on last non-blank symbol if it exists. Otherwise on a blank, one left of start position.

                    //accept if tape1 is shorter
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddReadCondition(tape2, 0, 1);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = accept;
                    M.AddTransition(pred, t);
                    //reject if tape2 is shorter
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape2, M.blank);
                    t.AddReadCondition(tape1, 0, 1);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = reject;
                    M.AddTransition(pred, t);
                    //enter state q1 if tapes have same length
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddReadCondition(tape2, M.blank);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    q = M.CreateState(M.Q.Count);
                    t.TargetState = q;
                    M.AddTransition(pred, t);
                    //default: move right
                    t = new Transition<int, int>();
                    t.AddMoveAction(tape1, Directions.Right);
                    t.AddMoveAction(tape2, Directions.Right);
                    M.AddDefaultTransition(pred, t);
                    #endregion

                    #region state q
                    //already determined: tapes have the same length
                    //compare tapes right to left
                    //reject at first occurrence of (tape1 > tape2)
                    //accept at first occurrence of (tape1 < tape2)
                    //reject if left end of both tapes is reached
                    //for each tape: end on some non-blank symbol

                    //reject if left end of both tapes is reached
                    //move right
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddReadCondition(tape2, M.blank);
                    t.AddMoveAction(tape1, Directions.Right);
                    t.AddMoveAction(tape2, Directions.Right);
                    t.TargetState = reject;
                    M.AddTransition(q, t);
                    //reject if tape1 > tape2
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, 1);
                    t.AddReadCondition(tape2, 0);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = reject;
                    M.AddTransition(q, t);
                    //accept if tape1 < tape2
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, 0);
                    t.AddReadCondition(tape2, 1);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = accept;
                    M.AddTransition(q, t);
                    //default: move left
                    t = new Transition<int, int>();
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    M.AddDefaultTransition(q, t);
                    #endregion

                    #endregion
                }

                if (op == CompareType.leq || op == CompareType.g)
                {
                    #region Less Than or Equal
                    #region state pred
                    //move right until the end of at least one tape is reached.
                    //accept if tape1 is shorter.
                    //reject if tape2 is shorter.
                    //if tapes have the same length: -> q
                    //for each tape: stops on last non-blank symbol if it exists. Otherwise on a blank, one left of start position.

                    //accept if tape1 is shorter
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddReadCondition(tape2, 0, 1);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = accept;
                    M.AddTransition(pred, t);
                    //reject if tape2 is shorter
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, 0, 1);
                    t.AddReadCondition(tape2, M.blank);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = reject;
                    M.AddTransition(pred, t);
                    //enter state q if tapes have same length
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddReadCondition(tape2, M.blank);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    q = M.CreateState(M.Q.Count);
                    t.TargetState = q;
                    M.AddTransition(pred, t);
                    //default: move right
                    t = new Transition<int, int>();
                    t.AddMoveAction(tape1, Directions.Right);
                    t.AddMoveAction(tape2, Directions.Right);
                    M.AddDefaultTransition(pred, t);
                    #endregion

                    #region state q
                    //already determined: tapes have the same length
                    //compare tapes right to left
                    //reject at first occurrence of (tape1 > tape2)
                    //[optional] accept at first occurrence of (tape1 < tape2)
                    //accept if left end of both tapes is reached
                    //for each tape: end on some non-blank symbol

                    //accept if left end of both tapes is reached
                    //move right
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddReadCondition(tape2, M.blank);
                    t.AddMoveAction(tape1, Directions.Right);
                    t.AddMoveAction(tape2, Directions.Right);
                    t.TargetState = accept;
                    M.AddTransition(q, t);
                    //reject if tape1 > tape2
                    //move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, 1);
                    t.AddReadCondition(tape2, 0);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = reject;
                    M.AddTransition(q, t);

                    #region OPTIONAL
                    //accept if tape1 < tape2
                    //move left
                    //tradeoff: TM takes fewer steps if tape1 < tape2, but has one additional transition
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, 0);
                    t.AddReadCondition(tape2, 1);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    t.TargetState = accept;
                    M.AddTransition(q, t);
                    #endregion

                    //default: move left
                    t = new Transition<int, int>();
                    t.AddMoveAction(tape1, Directions.Left);
                    t.AddMoveAction(tape2, Directions.Left);
                    M.AddDefaultTransition(q, t);
                    #endregion

                    #endregion
                }

                #region accept, reject
                //rewind both tapes simlutaneously, -> succTrue / succFalse
                addGoToEnd(Directions.Left, M, accept, succTrue, tape1, tape2);
                addGoToEnd(Directions.Left, M, reject, succFalse, tape1, tape2);

                #endregion
            }
            #endregion

            #region arg2 is a constant
            else
            {
                int c = ((WEConst)arg2).value;
                //convert LEQ to Less
                if(op == CompareType.leq || op == CompareType.g)
                {
                    ++c;
                }
                bool[] bits = WhileUtilities.IntToBitsBool(c);
                p = pred;

                #region Equals
                if(op == CompareType.eq || op == CompareType.neq)
                {
                    //check tape == const bit by bit
                    for (int i = 0; i < bits.Length; ++i)
                    {
                        q = M.CreateState(M.Q.Count);
                        //if tape1 reads current bit
                        //move right, p -> q
                        t = new Transition<int, int>();
                        t.AddReadCondition(tape1, bits[i] ? 1 : 0);
                        t.AddMoveAction(tape1, Directions.Right);
                        t.TargetState = q;
                        M.AddTransition(p, t);
                        //default: reject, move left
                        t = new Transition<int, int>();
                        t.AddMoveAction(tape1, Directions.Left);
                        t.TargetState = reject;
                        M.AddDefaultTransition(p, t);

                        p = q;
                    }

                    //accept if end of tape, move left
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddMoveAction(tape1, Directions.Left);
                    t.TargetState = accept;
                    M.AddTransition(p, t);
                    //default: reject, move left
                    t = new Transition<int, int>();
                    t.AddMoveAction(tape1, Directions.Left);
                    t.TargetState = reject;
                    M.AddDefaultTransition(p, t);

                }
                #endregion

                #region LEQ / Less
                else
                {
                    #region pred
                    //go to right end, -> p
                    p = M.CreateState(M.Q.Count);
                    addGoToEnd(Directions.Right, M, pred, p, tape1);
                    #endregion

                    #region bitwise comparison
                    presumeTrue = M.CreateState(M.Q.Count);
                    presumeFalse = M.CreateState(M.Q.Count);
                    int index;
                    for (index = bits.Length - 1; index >= 0; --index)
                    {
                        q = M.CreateState(M.Q.Count);
                        r = M.CreateState(M.Q.Count);
                        s = M.CreateState(M.Q.Count);

                        //find first difference between tape and const, from right to left

                        //if blank: R, -> succTrue
                        t = new Transition<int, int>();
                        t.AddReadCondition(tape1, M.blank);
                        t.AddMoveAction(tape1, Directions.Right);
                        t.TargetState = succTrue;
                        M.AddTransition(p, t);

                        //if tape1 < const (excluding blank): L, -> presumeTrue
                        if (bits[index])
                        {
                            t = new Transition<int, int>();
                            t.AddReadCondition(tape1, 0);
                            t.AddMoveAction(tape1, Directions.Left);
                            t.TargetState = presumeTrue;
                            M.AddTransition(p, t);
                        }
                        //if tape1 > const: L, -> presumeFalse
                        if (!bits[index])
                        {
                            t = new Transition<int, int>();
                            t.AddReadCondition(tape1, 1);
                            t.AddMoveAction(tape1, Directions.Left);
                            t.TargetState = presumeFalse;
                            M.AddTransition(p, t);
                        }
                        //default: L, -> q
                        t = new Transition<int, int>();
                        t.AddMoveAction(tape1, Directions.Left);
                        t.TargetState = q;
                        M.AddDefaultTransition(p, t);
                        p = q;

                        //difference was found: read remaining bits until blank
                        t = new Transition<int, int>();
                        t.AddReadCondition(tape1, M.blank);
                        t.AddMoveAction(tape1, Directions.Right);
                        t.TargetState = succTrue;
                        M.AddTransition(presumeTrue, t);

                        t = new Transition<int, int>();
                        t.AddMoveAction(tape1, Directions.Left);
                        t.TargetState = r;
                        M.AddDefaultTransition(presumeTrue, t);

                        //difference was found: read remaining bits until blank
                        t = new Transition<int, int>();
                        t.AddReadCondition(tape1, M.blank);
                        t.AddMoveAction(tape1, Directions.Right);
                        t.TargetState = succTrue;
                        M.AddTransition(presumeFalse, t);

                        t = new Transition<int, int>();
                        t.AddMoveAction(tape1, Directions.Left);
                        t.TargetState = s;
                        M.AddDefaultTransition(presumeFalse, t);

                        presumeTrue = r;
                        presumeFalse = s;
                    }
                    #endregion

                    #region after last bit
                    //no difference was found, so tape >= const. Reject.
                    t = new Transition<int, int>();
                    t.TargetState = reject;
                    M.AddDefaultTransition(p, t);

                    //presumeTrue
                    //if blank: R, -> succTrue
                    t = new Transition<int, int>();
                    t.AddReadCondition(tape1, M.blank);
                    t.AddMoveAction(tape1, Directions.Right);
                    t.TargetState = succTrue;
                    M.AddTransition(presumeTrue, t);
                    //else: reject
                    t = new Transition<int, int>();
                    t.TargetState = reject;
                    M.AddDefaultTransition(presumeTrue, t);

                    //presumeFalse
                    //reject
                    t = new Transition<int, int>();
                    t.TargetState = reject;
                    M.AddDefaultTransition(presumeFalse, t);
                    #endregion

                }

                #endregion

                #region accept, reject
                //rewind, -> succTrue / succFalse
                addGoToEnd(Directions.Left, M, accept, succTrue, tape1);
                addGoToEnd(Directions.Left, M, reject, succFalse, tape1);
                #endregion

            }
            #endregion
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            arg1.CollectVariables(vars, usefulVars, true);
            arg2.CollectVariables(vars, usefulVars, true);
        }

        public override XElement ToXML()
        {
            return new XElement("compare", new XAttribute("op", op.ToString()), arg1.ToXML(), arg2.ToXML());
        }

    }

    public class WCCompound : WCond
    {
        public enum Logic { and, or }
        public WCond cond1;
        public Logic op;
        public WCond cond2;

        public WCCompound(WCond cond1, Logic op, WCond cond2)
        {
            this.cond1 = cond1;
            this.op = op;
            this.cond2 = cond2;
        }

        public override CWExpr GetCWE()
        {
            CWCond cond1choice = (CWCond)cond1.GetCWE();
            CWCond cond2choice = (CWCond)cond2.GetCWE();
            return new CWCCompound(cond1choice, cond2choice);
        }

        public override string ToString()
        {
            string opStr = "";

            switch (op)
            {
                case Logic.and: opStr = "&&"; break;
                case Logic.or: opStr = "||"; break;
            }

            return $"({cond1.ToString()}) {opStr} ({cond2.ToString()})";
        }

        public override void BuildTMCB(State<int> pred, State<int> succTrue, State<int> succFalse, TMCB<int, int> M)
        {
            State<int> q;
            //State<int> p;

            switch (op)
            {
                case Logic.and:

                    q = M.CreateState(M.Q.Count);
                    cond1.BuildTMCB(pred, q, succFalse, M);
                    cond2.BuildTMCB(q, succTrue, succFalse, M);

                    break;

                case Logic.or:

                    q = M.CreateState(M.Q.Count);
                    cond1.BuildTMCB(pred, succTrue, q, M);
                    cond2.BuildTMCB(q, succTrue, succFalse, M);

                    break;

                //'implies' and 'iff' are currently unused.

                //case Logic.iff:

                //    q = M.CreateState(M.Q.Count);
                //    p = M.CreateState(M.Q.Count);
                //    cond1.BuildTMCB(pred, q, p, M);
                //    cond2.BuildTMCB(q, succTrue, succFalse, M);
                //    cond2.BuildTMCB(p, succFalse, succTrue, M);

                //    break;

                //case Logic.implies:

                //    q = M.CreateState(M.Q.Count);
                //    cond1.BuildTMCB(pred, q, succTrue, M);
                //    cond2.BuildTMCB(q, succTrue, succFalse, M);

                //    break;

            }
        }

        public override void CollectVariables(HashSet<int> vars, HashSet<int> usefulVars, bool useful)
        {
            cond1.CollectVariables(vars, usefulVars, false);
            cond2.CollectVariables(vars, usefulVars, false);
        }

        public override XElement ToXML()
        {
            return new XElement("compound", new XAttribute("op", op.ToString()), cond1.ToXML(), cond2.ToXML());
        }

    }
}
