using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.PDA
{
    public class TransitionBuilder<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        PDA<A, S> pda;

        internal TransitionBuilder(PDA<A, S> pda)
        {
            this.pda = pda;
        }

        static internal FromBuilder BuildTrasition(PDA<A, S> pda)
        {
            return new FromBuilder(new TransitionBuilder<A, S>(pda));
        }

        int startId;
        int targetId;
        Symbol<A> symbolIn;
        S stackSymbolIn;
        S[] stackSymbolsWritten;

        public class Builder
        {
            protected TransitionBuilder<A, S> builder;

            public Builder(TransitionBuilder<A, S> builder)
            {
                this.builder = builder;
            }
        }

        public class FromBuilder : Builder
        {
            internal FromBuilder(TransitionBuilder<A, S> builder) : base(builder)
            {
            }

            /// <summary>
            /// set the start-node of the transition
            /// </summary>
            /// <param name="id">id of the start-node of the transition</param>
            /// <returns>builder for adding target-node</returns>
            public ToBuilder From(int id)
            {
                builder.startId = id;
                return new ToBuilder(builder);
            }
        }

        public class ToBuilder : Builder
        {
            internal ToBuilder(TransitionBuilder<A, S> builder) : base(builder)
            {
            }

            /// <summary>
            /// set the target-node of the transition
            /// </summary>
            /// <param name="id">id of the target-node of the transition</param>
            /// <returns>builder for adding the read symbol</returns>
            public ReadBuilder To(int id)
            {
                builder.targetId = id;
                return new ReadBuilder(builder);
            }
        }

        public class ReadBuilder : Builder
        {
            internal ReadBuilder(TransitionBuilder<A, S> builder) : base(builder)
            {
            }

            /// <summary>
            /// set the input symbol
            /// </summary>
            /// <param name="symbol">input symbol of the transition</param>
            /// <returns>builder for adding the input stack-symbol</returns>
            public PopBuilder Read(A symbol)
            {
                builder.symbolIn = Symbol<A>.SymbolIn(symbol);
                return new PopBuilder(builder);
            }

            /// <summary>
            /// set the input symbol to epsilon
            /// </summary>
            /// <returns>builder for adding the input stack-symbol</returns>
            public PopBuilder Read()
            {
                builder.symbolIn = Symbol<A>.EpsilonIn();
                return new PopBuilder(builder);
            }
            
            /// <summary>
            /// set the input symbol
            /// </summary>
            /// <returns>builder for adding the input stack-symbol</returns>
            internal PopBuilder Read(Symbol<A> symbolIn)
            {
                builder.symbolIn = symbolIn;
                return new PopBuilder(builder);
            }
        }

        public class PopBuilder : Builder
        {
            internal PopBuilder(TransitionBuilder<A, S> builder) : base(builder)
            {
            }

            /// <summary>
            /// set the input stack-symbol
            /// </summary>
            /// <param name="symbol">input stack-symbol of the transition</param>
            /// <returns>builder for adding the written stack-symbols</returns>
            public PushBuilder Pop(S symbol)
            {
                builder.stackSymbolIn = symbol;
                return new PushBuilder(builder);
            }
        }

        public class PushBuilder : Builder
        {
            internal PushBuilder(TransitionBuilder<A, S> builder) : base(builder)
            {
            }

            /// <summary>
            /// set the written stack-symbols
            /// </summary>
            /// <param name="symbolsTopToDown">written stack-sybols of the transition, where the first symbol of the list is the top most of the stack</param>
            public void Push(IEnumerable<S> symbolsTopToDown)
            {
                builder.stackSymbolsWritten = symbolsTopToDown.ToArray();
                builder.pda.AddTransition(builder.startId, builder.targetId, builder.symbolIn, builder.stackSymbolIn, builder.stackSymbolsWritten);
            }
            
            /// <summary>
            /// set the written stack-symbols to empty array
            /// </summary>
            public void Push()
            {
                builder.stackSymbolsWritten = new S[] { };
                builder.pda.AddTransition(builder.startId, builder.targetId, builder.symbolIn, builder.stackSymbolIn, builder.stackSymbolsWritten);
            }
        }
    }
}
