using Microsoft.Z3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.WhileProgram
{
    /* class for generating a set of while programs from one base program
     * 
     * Copied from AutomataPDL\PDL\ProblemGeneration and adapted for while programs.
     * 
     * 1) converts WExpr baseProgram -> CWExpr choiceTree,
     * 2) creates new WExpr's using a z3Solver,
     *    taking the specified constMode into account
     * 3) filters out unwanted WExpr's
     *    using the specified filterMode and ProgramBuilder.FilterModes
     *    
     * Each call to GenerateWExpr returns the next WExpr that passes the filters, as an output parameter.
     * Returns false iff all feasible WExpr's have already been generated.
     * In this case, Reset may be called to start over.
     */

    public class ProgramGenerator
    {
        private Context z3Context;
        private Solver z3Solver;
        private int maxNumVariables;
        private CWExpr choiceTree;
        private IEnumerable<string> choiceVariables;
        private WEFilter filter;
        private VariableCache.ConstraintMode constMode;
        private ProgramBuilder.FilterMode[] builderFilterModes;

        public ProgramGenerator(WExpr baseProgram, WEFilter.Filtermode filterMode, VariableCache.ConstraintMode constMode, params ProgramBuilder.FilterMode[] builderFilterModes)
        {
            this.maxNumVariables = baseProgram.GetNumVariables();
            this.choiceTree = baseProgram.GetCWE();
            this.filter = WEFilter.Create(filterMode, baseProgram);
            this.constMode = constMode;
            this.builderFilterModes = builderFilterModes;

            initialize();
        }

        private void initialize()
        {
            this.z3Context = new Context();
            this.z3Solver = this.z3Context.MkSolver();
            GenerateConstraints(choiceTree, maxNumVariables, constMode, z3Context, z3Solver);
            this.choiceVariables = choiceTree.GetChoiceVariables();
        }

        public void Reset()
        {
            initialize();
        }

        public bool GenerateWExpr(out WExpr program)
        {
            program = null;
            WExpr candidate;

            while (z3Solver.Check() == Status.SATISFIABLE)
            {
                ProgramBuilder builder = new ProgramBuilder(builderFilterModes);
                candidate = choiceTree.BuildProgram(z3Context, z3Solver.Model, builder);
                ExcludeLastModel(choiceVariables, z3Context, z3Solver);
                if (builder.KeepProgram() && filter.KeepProgram(candidate))
                {
                    program = candidate;
                    break;
                }
            }

            return program != null;
        }

        private static void GenerateConstraints(CWExpr choiceExpr, int maxNumVariables, VariableCache.ConstraintMode constraintMode, Context z3Context, Solver z3Solver)
        {
            VariableCache variableGenerator = VariableCache.Create(constraintMode);
            choiceExpr.ToSMTConstraints(z3Context, z3Solver, maxNumVariables, variableGenerator);
            variableGenerator.GenerateAdditionalConstraints(z3Context, z3Solver);
        }

        private static void ExcludeLastModel(IEnumerable<string> choiceVariables, Context z3Context, Solver z3Solver)
        {
            Model lastModel = z3Solver.Model;
            BoolExpr characteristicFormula = CreateCharacteristicFormula(choiceVariables, z3Context, lastModel);
            z3Solver.Assert(z3Context.MkNot(characteristicFormula));
        }

        private static BoolExpr CreateCharacteristicFormula(IEnumerable<string> choiceVariables, Context z3Context, Model lastModel)
        {
            BoolExpr characteristicFormula = null;
            foreach (string choiceVariable in choiceVariables)
            {
                BoolExpr currentAssignment = CreateAssignmentFormula(z3Context, lastModel, choiceVariable);
                if (characteristicFormula == null)
                {
                    characteristicFormula = currentAssignment;
                }
                else
                {
                    characteristicFormula = z3Context.MkAnd(characteristicFormula, currentAssignment);
                }
            }
            return characteristicFormula;
        }

        private static BoolExpr CreateAssignmentFormula(Context z3Context, Model lastModel, string choiceVariable)
        {
            ArithExpr z3Variable = z3Context.MkIntConst(choiceVariable);
            ArithExpr assignment = (ArithExpr)lastModel.ConstInterp(z3Variable);
            BoolExpr currentAssignment = z3Context.MkEq(z3Variable, assignment);
            return currentAssignment;
        }

    }

    public abstract class VariableCache
    {
        public enum ConstraintMode
        {
            NONE, // Generated constants follow no relation among each other
        }

        protected int nextVariableNumber = 0;
        protected IDictionary<int, string> constChoiceVariables = new Dictionary<int, string>();
        protected IDictionary<int, string> varChoiceVariables = new Dictionary<int, string>();

        private VariableConstraintGenerator constraintGenerator;

        public static VariableCache Create(ConstraintMode constraintMode)
        {
            switch (constraintMode)
            {
                case ConstraintMode.NONE: return new UnconstrainedVariableCache(new NoConstraintGenerator());
            }
            return null;
        }

        protected VariableCache(VariableConstraintGenerator constraintGenerator)
        {
            this.constraintGenerator = constraintGenerator;
        }

        public ICollection<string> GetConstVariables()
        {
            return new List<string>(this.constChoiceVariables.Values);
        }

        public ICollection<string> GetVarVariables()
        {
            return new List<string>(this.varChoiceVariables.Values);
        }

        abstract public string GetFreshVariableName();
        abstract public string GetConstChoiceVariable(int originalValue);
        abstract public string GetVarChoiceVariable(int originalValue);

        public void GenerateAdditionalConstraints(Context z3Context, Solver z3Solver)
        {
            this.constraintGenerator.GenerateVariableConstraints(z3Context, z3Solver);
        }

        internal int GetNumVariables()
        {
            return this.nextVariableNumber;
        }
    }

    public class UnconstrainedVariableCache : VariableCache
    {
        public UnconstrainedVariableCache(VariableConstraintGenerator constraintGenerator)
            : base(constraintGenerator) { }

        private string getFreshVariableName(string prefix)
        {
            string returnValue = prefix + this.nextVariableNumber.ToString();
            this.nextVariableNumber += 1;
            return returnValue;
        }

        public override string GetFreshVariableName()
        {
            return this.getFreshVariableName("choiceVar_");
        }

        public override string GetVarChoiceVariable(int originalValue)
        {
            return this.getFreshVariableName("var_");
        }

        public override string GetConstChoiceVariable(int originalValue)
        {
            return this.getFreshVariableName("const_");
        }
    }

    public abstract class VariableConstraintGenerator
    {
        public abstract void GenerateVariableConstraints(Context z3Context, Solver z3Solver);
    }

    public class NoConstraintGenerator : VariableConstraintGenerator
    {
        public override void GenerateVariableConstraints(Context z3Context, Solver z3Solver)
        {
            // Since we do not need any constraints, just do nothing. Basically a null-object
            return;
        }
    }

    public abstract class WEFilter
    {
        protected const int numTestInputs = 100;

        public enum Filtermode
        {
            NONE,   // Generated programs are not filtered at all
            INPUTS, // Generated programs are filtered if too few inputs get altered by a run of the corresponding TM
            INFINITE, // Generated programs are filtered if too many inputs timeout on the corresponding TM
        }

        public static WEFilter Create(Filtermode filtermode, WExpr original)
        {
            switch (filtermode)
            {
                case Filtermode.NONE: return new NoFilter();
                case Filtermode.INPUTS: return new InputsFilter();
                case Filtermode.INFINITE: return new LongExecutionsFilter();
            }
            return null;
        }

        public abstract bool KeepProgram(WExpr candidate);
    }

    public class NoFilter : WEFilter
    {
        public override bool KeepProgram(WExpr candidate) { return true; }
    }

    /* Needs tweaking. Current approach relies on percentages, so programs like 
     *  if (x < 5) then x++
     * are filtered out, but they should be kept, at least for low difficulty.
     */
    public class InputsFilter : WEFilter
    {
        public override bool KeepProgram(WExpr candidate)
        {
            Automata.TMCB<int, int> M = candidate.ToTMCB(-1);
            int numTapes = candidate.GetNumVariables();
            int inputsPerTape = (int)Math.Pow(numTestInputs, (double)1 / numTapes);

            bool dummy;
            int[][] output;
            int count = 0; //counts inputs that get modified by a run of the TM
            int total = 0;
            foreach(int[][] input in WhileUtilities.NonNegIntTestInputs(numTapes, inputsPerTape, candidate.GetUselessVariables().ToArray()))
            {
                output = M.Run(input, out dummy);
                if(!WhileUtilities.TapesEqual(Array.ConvertAll(input, tape => WhileUtilities.TapeToString(tape, M.blank, null)), Array.ConvertAll(output, tape => WhileUtilities.TapeToString(tape, M.blank, null))))
                {
                    ++count;
                }
                ++total;
            }
            return decideByPercentage(count, total);
        }

        private bool decideByPercentage(int count, int total)
        {
            double threshold = 0.2;
            return (double)count / total >= threshold;
        }


    }

    public class LongExecutionsFilter : WEFilter
    {
        public override bool KeepProgram(WExpr candidate)
        {
            Automata.TMCB<int, int> M = candidate.ToTMCB(-1);
            int numTapes = candidate.GetNumVariables();
            int inputsPerTape = (int)Math.Pow(numTestInputs, (double)1 / numTapes);

            bool dummy;
            int count = 0; //counts inputs for which the TM times out
            int total = 0;
            foreach (int[][] input in WhileUtilities.NonNegIntTestInputs(numTapes, inputsPerTape, candidate.GetUselessVariables().ToArray()))
            {
                M.Run(input, out dummy);
                if (M.Timeout)
                {
                    ++count;
                }
                ++total;
            }
            return decideByPercentage(count, total);
        }

        private bool decideByPercentage(int count, int total)
        {
            return (double)count / total <= 0.1;
        }
    }
}
