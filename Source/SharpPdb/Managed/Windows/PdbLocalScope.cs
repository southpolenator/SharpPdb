using SharpPdb.Windows.SymbolRecords;
using SharpUtilities;
using System.Collections.Generic;
using System.Linq;

namespace SharpPdb.Managed.Windows
{
    /// <summary>
    /// Represents local scope in Windows PDB file.
    /// </summary>
    public class PdbLocalScope : IPdbLocalScope
    {
        /// <summary>
        /// Cache of <see cref="Children"/> property.
        /// </summary>
        private SimpleCacheStruct<IPdbLocalScope[]> childrenCache;

        /// <summary>
        /// Cache of <see cref="Constants"/> property.
        /// </summary>
        private SimpleCacheStruct<IPdbLocalConstant[]> constantsCache;

        /// <summary>
        /// Cache of <see cref="Variables"/> property.
        /// </summary>
        private SimpleCacheStruct<IPdbLocalVariable[]> variablesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbLocalScope"/> class.
        /// </summary>
        /// <param name="function">Function that contains this scope.</param>
        /// <param name="block">Block symbol read from PDB.</param>
        /// <param name="parent">Parent scope.</param>
        internal PdbLocalScope(PdbFunction function, BlockSymbol block, PdbLocalScope parent = null)
        {
            Function = function;
            Block = block;
            Parent = parent;
            childrenCache = SimpleCache.CreateStruct(() =>
            {
                IEnumerable<BlockSymbol> blocks = Block.Children.OfType<BlockSymbol>();
                int count = blocks.Count();
                IPdbLocalScope[] children = new IPdbLocalScope[count];
                int i = 0;

                foreach (BlockSymbol b in blocks)
                    children[i++] = new PdbLocalScope(Function, b, this);
                return children;
            });
            constantsCache = SimpleCache.CreateStruct(() =>
            {
                IEnumerable<ConstantSymbol> symbols = Block.Children.OfType<ConstantSymbol>();
                int count = symbols.Count();
                IPdbLocalConstant[] constants = new IPdbLocalConstant[count];
                int i = 0;

                foreach (ConstantSymbol symbol in symbols)
                    constants[i++] = new PdbLocalConstant(this, symbol);
                return constants;
            });
            variablesCache = SimpleCache.CreateStruct(() =>
            {
                IEnumerable<AttributeSlotSymbol> slots = Block.Children.OfType<AttributeSlotSymbol>();
                int count = slots.Count();
                IPdbLocalVariable[] variables = new IPdbLocalVariable[count];
                int i = 0;

                foreach (AttributeSlotSymbol slot in slots)
                    variables[i++] = new PdbLocalVariable(this, slot);
                return variables;
            });
        }

        /// <summary>
        /// Gets the function that contains this local scope.
        /// </summary>
        public PdbFunction Function { get; private set; }

        /// <summary>
        /// Gets the block symbol.
        /// </summary>
        public BlockSymbol Block { get; private set; }

        /// <summary>
        /// Gets the IL offset of the local scope start.
        /// </summary>
        public int StartOffset => (int)(Block.CodeOffset - Function.Procedure.CodeOffset);

        /// <summary>
        /// Gets the IL offset of the local scope end.
        /// </summary>
        public int EndOffset => StartOffset + Length;

        /// <summary>
        /// Gets the length of the local scope.
        /// </summary>
        public int Length => (int)Block.CodeSize;

        /// <summary>
        /// Gets the function that contains this local scope.
        /// </summary>
        IPdbFunction IPdbLocalScope.Function => Function;

        /// <summary>
        /// Gets the parent local scope or <c>null</c> if it doesn't exist.
        /// </summary>
        public IPdbLocalScope Parent { get; private set; }

        /// <summary>
        /// Gets the list of contained local scopes.
        /// </summary>
        public IReadOnlyList<IPdbLocalScope> Children => childrenCache.Value;

        /// <summary>
        /// Gets the list of constants declared in this local scope.
        /// </summary>
        public IReadOnlyList<IPdbLocalConstant> Constants => constantsCache.Value;

        /// <summary>
        /// Gets the list of variables declared in this local scope.
        /// </summary>
        public IReadOnlyList<IPdbLocalVariable> Variables => variablesCache.Value;
    }
}
