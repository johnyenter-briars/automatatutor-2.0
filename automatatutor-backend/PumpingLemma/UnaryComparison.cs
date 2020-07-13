using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PumpingLemma
{
    public class UnaryComparison
    {
        public VariableType variable;

        public enum ComparisonType
        {
            EQUAL,
            BETWEEN,
            GREATER
        }
        public ComparisonType comparisonType;

        public int constant;

        public int min;
        public int max;

        IEnumerable<int> neq;

        private UnaryComparison(ComparisonType type, int c1, int c2, VariableType variable, IEnumerable<int> neq)
        {
            this.comparisonType = type;
            switch(type)
            {
                case ComparisonType.EQUAL:
                    constant = c1;
                    this.variable = variable;
                    this.neq = neq;
                    break;
                case ComparisonType.GREATER:
                    constant = c1;
                    min = c1;
                    this.variable = variable;
                    this.neq = neq;
                    break;
                case ComparisonType.BETWEEN:
                    min = c1;
                    max = c2;
                    this.variable = variable;
                    this.neq = neq;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            switch(comparisonType)
            {
                case ComparisonType.EQUAL:
                    return variable.ToString() + " == " + constant;
                case ComparisonType.GREATER:
                    sb.Append(variable.ToString() + " > " + constant);
                    foreach (int i in neq) sb.Append(" && " + variable.ToString() + " != " + i);
                    return sb.ToString();
                case ComparisonType.BETWEEN:
                    if (min == -1)
                        sb.Append(variable.ToString() + " < " + max);
                    else
                        sb.Append(min + " < " + variable.ToString() + " < " + max);
                    foreach (int i in neq) sb.Append(" && " + variable.ToString() + " != " + i);
                    return sb.ToString();
                default:
                    throw new ArgumentException();
            }
        }

        public static UnaryComparison equal(VariableType variable, int constant)
        {
            return new UnaryComparison(ComparisonType.EQUAL, constant, constant, variable, new HashSet<int>());
        }
        public static UnaryComparison greater(VariableType variable, int constant, IEnumerable<int> neq)
        {
            return new UnaryComparison(ComparisonType.GREATER, constant, constant, variable, neq);
        } 
        public static UnaryComparison between(VariableType variable, int min, int max, IEnumerable<int> neq)
        {
            if (min == max - 2) return new UnaryComparison(ComparisonType.EQUAL, min+1, min+1, variable, neq);
            return new UnaryComparison(ComparisonType.BETWEEN, min, max, variable, neq);
        }
    }
}
