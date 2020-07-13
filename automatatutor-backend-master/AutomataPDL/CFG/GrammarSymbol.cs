using System;


namespace AutomataPDL.CFG
{
    public abstract class GrammarSymbol
    {
        public abstract string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        private int uniqueId;
        public int UnqiueId {
            get
            {
                if (uniqueId == 0)
                {
                    throw new Exception("The id is unset");
                }
                return uniqueId;
            }
            set
            {
                uniqueId = value;
            }
        }
    }
}
