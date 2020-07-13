using Microsoft.Z3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.WhileProgram
{
    /* base class for "choice while expressions"
     * conversion rules WExpr -> CWExpr:
     * WEVar    -> CWEVar
     * WEConst  -> CWEConst
     * WEConcat -> CWEConcat
     * WEWhile  -> CWEWhile
     * WEIf     -> CWEIf
     * WEArith  -> CWEArithSimple
     *          |  CWEAssign
     *          |  CWEIncDec
     * WCNot    -> CWCSign
     * WCComparison -> CWCSimple
     *              |  CWCComplex
     * WCCompound   -> CWCCompound
     */
    public abstract class CWExpr
    {
        protected string constraintVariable;

        /* Traverses the choice tree, creating constraints in the <z3Context>.
         * Stores identifiers in <constraintVariable>.
         * <maxNumVars> is the number of distinct WEVars in the original program. Only used in CWEVar.ToSMTConstraints.
         */
        public abstract void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator);

        public ICollection<string> GetChoiceVariables()
        {
            return this.CollectChoiceVariables(new HashSet<string>());
        }

        // Traverses the choice tree, collecting <constraintVariable>s
        public abstract HashSet<string> CollectChoiceVariables(HashSet<string> currentVals);

        public WExpr BuildProgram(Context context, Model model, ProgramBuilder builder)
        {
            WExpr rv = InterpretModel(context, model, builder);
            builder.FinalizeProgram();
            return rv;
        }

        // Traverses the choice tree, building WExprs from the individual CWExprs
        public abstract WExpr InterpretModel(Context context, Model model, ProgramBuilder builder);

        // Gets the concrete choice for this CWExpr's <constraintVariable> in the specified <model> and <context>
        protected int GetConcChoice(Context context, Model model)
        {
            int concChoice = ((IntNum)model.ConstInterp(context.MkIntConst(this.constraintVariable))).Int;
            return concChoice;
        }

    }

    public abstract class CWEOperand : CWExpr
    {

    }

    public class CWEVar : CWEOperand
    {
        private int orgId;

        public CWEVar(int orgId)
        {
            this.orgId = orgId;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            return currentVals;
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WExpr rv = builder.NewVar(this.GetConcChoice(context, model));
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            this.constraintVariable = variableGenerator.GetVarChoiceVariable(orgId);
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(maxNumVars - 1)));
        }
    }

    public class CWEConst : CWEOperand
    {
        private int orgValue;

        public CWEConst(int orgValue)
        {
            this.orgValue = orgValue;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            return currentVals;
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WExpr rv = builder.NewConst(this.GetConcChoice(context, model));
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            this.constraintVariable = variableGenerator.GetConstChoiceVariable(orgValue);
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            //range for variation of constants
            //TODO: allow 0?
            int lowerBound = Math.Max((int)(this.orgValue / 2), 0);
            int upperBound = Math.Max((int)(this.orgValue * 2), 0);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(lowerBound), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(upperBound)));
        }
    }

    public class CWEConcat : CWExpr
    {
        private CWExpr expr1;
        private CWExpr expr2;

        public CWEConcat(CWExpr expr1, CWExpr expr2)
        {
            this.expr1 = expr1;
            this.expr2 = expr2;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(constraintVariable);
            expr1.CollectChoiceVariables(currentVals);
            return expr2.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WExpr rv = new WEConcat(expr1.InterpretModel(context, model, builder), expr2.InterpretModel(context, model, builder));
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkEq(z3Context.MkInt(0), myVariable));
            this.expr1.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.expr2.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }
    }

    public class CWEWhile : CWExpr
    {
        private CWCond condition;
        private CWExpr body;

        public CWEWhile(CWCond condition, CWExpr body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkEq(z3Context.MkInt(0), myVariable));
            this.condition.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.body.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            currentVals = this.condition.CollectChoiceVariables(currentVals);
            return this.body.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WExpr rv = new WEWhile((WCond)this.condition.InterpretModel(context, model, builder), this.body.InterpretModel(context, model, builder));
            builder.CheckExpr(rv);
            return rv;
        }
    }

    public class CWEIf : CWExpr
    {
        private CWCond condition;
        private CWExpr thenBody;
        private CWExpr elseBody;

        public CWEIf(CWCond condition, CWExpr thenBody, CWExpr elseBody)
        {
            this.condition = condition;
            this.thenBody = thenBody;
            this.elseBody = elseBody;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkEq(z3Context.MkInt(0), myVariable));
            this.condition.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.thenBody.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            if(elseBody != null)
            {
                this.elseBody.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            }
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(constraintVariable);
            currentVals = condition.CollectChoiceVariables(currentVals);
            if(elseBody != null)
            {
                elseBody.CollectChoiceVariables(currentVals);
            }
            return thenBody.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WExpr rv = new WEIf((WCond)this.condition.InterpretModel(context, model, builder), this.thenBody.InterpretModel(context, model, builder), (elseBody != null) ? this.elseBody.InterpretModel(context, model, builder) : null);
            builder.CheckExpr(rv);
            return rv;
        }

    }

    public class CWEAssign : CWExpr
    {
        private CWEVar lhs;
        private CWEOperand assign;

        public CWEAssign(CWEVar lhs, CWEOperand assign)
        {
            this.lhs = lhs;
            this.assign = assign;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            lhs.CollectChoiceVariables(currentVals);
            return assign.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WEVar lhsConc = (WEVar)lhs.InterpretModel(context, model, builder);
            WEOperand assignConc = (WEOperand)assign.InterpretModel(context, model, builder);

            WExpr rv = new WEArith(lhsConc, assignConc, WEArith.ArithOp.plus, 0);
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            //contraint Variable = 0
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(0)));
            this.lhs.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.assign.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }
    }

    public class CWEIncDec : CWExpr
    {
        private CWEVar arg;

        public CWEIncDec(CWEVar arg)
        {
            this.arg = arg;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            return arg.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WEVar argConc = (WEVar)arg.InterpretModel(context, model, builder);
            WEArith.ArithOp op = (WEArith.ArithOp)GetConcChoice(context, model);

            WExpr rv = new WEArith(argConc, argConc, op, builder.NewConst(1));
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            /* constraint variable in { 0, 1 }
             * 0 -> increment
             * 1 -> decrement
             */
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(1)));
            this.arg.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }
    }

    public class CWEArithSimple : CWExpr
    {
        private CWEVar lhs;
        private CWEOperand arg1;
        private CWEOperand arg2;

        public CWEArithSimple(CWEVar lhs, CWEOperand arg1, CWEOperand arg2)
        {
            this.lhs = lhs;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            /* constraint variable in { 0, 1 }
             * 0 -> plus
             * 1 -> minus
             */
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(Enum.GetNames(typeof(WEArith.ArithOp)).Length - 1)));
            this.lhs.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.arg1.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.arg2.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            lhs.CollectChoiceVariables(currentVals);
            arg1.CollectChoiceVariables(currentVals);
            return arg2.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WEVar lhsConc = (WEVar)lhs.InterpretModel(context, model, builder);
            WEArith.ArithOp op = (WEArith.ArithOp)GetConcChoice(context, model);
            WEOperand arg1Conc = (WEOperand)arg1.InterpretModel(context, model, builder);
            WEOperand arg2Conc = (WEOperand)arg2.InterpretModel(context, model, builder);

            WExpr rv = new WEArith(lhsConc, arg1Conc, op, arg2Conc);
            builder.CheckExpr(rv);
            return rv;
        }
    }

    //currently unused. Originally created for WEArith expressions with multiplication, division and modulo.
    public class CWEArithComplex : CWExpr
    {
        private CWEVar lhs;
        private CWEOperand arg1;
        private CWEOperand arg2;

        public CWEArithComplex(CWEVar lhs, CWEOperand arg1, CWEOperand arg2)
        {
            this.lhs = lhs;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(2)));
            this.lhs.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.arg1.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.arg2.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            lhs.CollectChoiceVariables(currentVals);
            arg1.CollectChoiceVariables(currentVals);
            return arg2.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WEVar lhsConc = (WEVar)lhs.InterpretModel(context, model, builder);
            WEArith.ArithOp op = (WEArith.ArithOp)(GetConcChoice(context, model) + 2);
            WEOperand arg1Conc = (WEOperand)arg1.InterpretModel(context, model, builder);
            WEOperand arg2Conc = (WEOperand)arg2.InterpretModel(context, model, builder);

            WExpr rv = new WEArith(lhsConc, arg1Conc, op, arg2Conc);
            builder.CheckExpr(rv);
            return rv;
        }
    }

    public abstract class CWCond : CWExpr
    {
    }

    //Currently unused. Originally used as abstraction of WCNot.
    public class CWCSign : CWCond
    {
        private CWCond condition;

        public CWCSign(CWCond condition)
        {
            this.condition = condition;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(this.constraintVariable);
            return condition.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WExpr rv;
            if (GetConcChoice(context, model) == 1)
            {
                rv = new WCNot((WCond)condition.InterpretModel(context, model, builder));
            }
            else
            {
                rv = (WCond)condition.InterpretModel(context, model, builder);
            }
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(1)));
            this.condition.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }
    }

    public class CWCSimple : CWCond
    {
        private CWEVar x;

        public CWCSimple(CWEVar x)
        {
            this.x = x;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(constraintVariable);
            return x.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            int opChoice = GetConcChoice(context, model);
            WCComparison.CompareType op = (opChoice == 0) ? WCComparison.CompareType.eq : WCComparison.CompareType.neq;
            WExpr rv = new WCComparison((WEVar)x.InterpretModel(context, model, builder), op, new WEConst(0));
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            /* constraint variable in { 0, 1 }
             * 0 -> eq
             * 1 -> neq
             */
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(1)));
            this.x.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }
    }

    public class CWCComplex : CWCond
    {
        private CWEVar arg1;
        private CWEOperand arg2;

        public CWCComplex(CWEVar arg1, CWEOperand arg2)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(constraintVariable);
            arg1.CollectChoiceVariables(currentVals);
            return arg2.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WEVar arg1Conc = (WEVar)arg1.InterpretModel(context, model, builder);
            WCComparison.CompareType comp = (WCComparison.CompareType)this.GetConcChoice(context, model);
            WEOperand arg2Conc = (WEOperand)arg2.InterpretModel(context, model, builder);
            WExpr rv = new WCComparison(arg1Conc, comp, arg2Conc);
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            /* constraint variable in { 0, ..., 5 }
             * -> eq, l, leq, neq, g, geq
             */

            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(Enum.GetNames(typeof(WCComparison.CompareType)).Length - 1)));
            this.arg1.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            //Currently, arg2 may be concretized to a 0 constant, possibly resulting in an '== 0' or '!= 0' condition, which is a simple condition.
            this.arg2.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }
    }

    public class CWCCompound : CWCond
    {
        CWCond cond1;
        CWCond cond2;

        public CWCCompound(CWCond cond1, CWCond cond2)
        {
            this.cond1 = cond1;
            this.cond2 = cond2;
        }

        public override HashSet<string> CollectChoiceVariables(HashSet<string> currentVals)
        {
            currentVals.Add(constraintVariable);
            cond1.CollectChoiceVariables(currentVals);
            return cond2.CollectChoiceVariables(currentVals);
        }

        public override WExpr InterpretModel(Context context, Model model, ProgramBuilder builder)
        {
            WCond cond1Conc = (WCond)cond1.InterpretModel(context, model, builder);
            WCCompound.Logic op = (WCCompound.Logic)GetConcChoice(context, model);
            WCond cond2Conc = (WCond)cond2.InterpretModel(context, model, builder);
            WExpr rv = new WCCompound(cond1Conc, op, cond2Conc);
            builder.CheckExpr(rv);
            return rv;
        }

        public override void ToSMTConstraints(Context z3Context, Solver z3Solver, int maxNumVars, VariableCache variableGenerator)
        {
            /* constraint variable in { 0, 1 }
             * 0 -> and
             * 1 -> or
             */
            this.constraintVariable = variableGenerator.GetFreshVariableName();
            ArithExpr myVariable = z3Context.MkIntConst(this.constraintVariable);
            z3Solver.Assert(z3Context.MkLe(z3Context.MkInt(0), myVariable));
            z3Solver.Assert(z3Context.MkLe(myVariable, z3Context.MkInt(Enum.GetNames(typeof(WCCompound.Logic)).Length - 1)));
            this.cond1.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
            this.cond2.ToSMTConstraints(z3Context, z3Solver, maxNumVars, variableGenerator);
        }
    }
}
