using SharpUtilities;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents local scope in Portable PDB file.
    /// </summary>
    public class PdbLocalScope : IPdbLocalScope
    {
        /// <summary>
        /// Cache of <see cref="LocalScope"/> property.
        /// </summary>
        private SimpleCacheStruct<LocalScope> localScopeCache;

        /// <summary>
        /// Cache of <see cref="Children"/> property.
        /// </summary>
        private SimpleCacheStruct<List<IPdbLocalScope>> childrenCache;

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
        /// <param name="handle">Our metadata reader handle.</param>
        /// <param name="parent">Parent scope.</param>
        internal PdbLocalScope(PdbFunction function, LocalScopeHandle handle, IPdbLocalScope parent = null)
        {
            Function = function;
            localScopeCache = SimpleCache.CreateStruct(() => function.PdbFile.Reader.GetLocalScope(handle));
            childrenCache = SimpleCache.CreateStruct(() =>
            {
                var enumerator = LocalScope.GetChildren();
                List<IPdbLocalScope> children = new List<IPdbLocalScope>();

                while (enumerator.MoveNext())
                {
                    children.Add(new PdbLocalScope(function, enumerator.Current, this));
                }
                return children;
            });
            constantsCache = SimpleCache.CreateStruct(() =>
            {
                var localConstants = LocalScope.GetLocalConstants();
                IPdbLocalConstant[] constants = new IPdbLocalConstant[localConstants.Count];
                int i = 0;

                foreach (var c in localConstants)
                    constants[i++] = new PdbLocalConstant(this, c);
                return constants;
            });
            variablesCache = SimpleCache.CreateStruct(() =>
            {
                var localVariables = LocalScope.GetLocalVariables();
                IPdbLocalVariable[] variables = new IPdbLocalVariable[localVariables.Count];
                int i = 0;

                foreach (var v in localVariables)
                    variables[i++] = new PdbLocalVariable(this, v);
                return variables;
            });
        }

        /// <summary>
        /// Gets the function that contains this local scope.
        /// </summary>
        public PdbFunction Function { get; private set; }

        /// <summary>
        /// Gets the local scope.
        /// </summary>
        public LocalScope LocalScope => localScopeCache.Value;

        /// <summary>
        /// Gets the IL offset of the local scope start.
        /// </summary>
        public int StartOffset => LocalScope.StartOffset;

        /// <summary>
        /// Gets the IL offset of the local scope end.
        /// </summary>
        public int EndOffset => LocalScope.EndOffset;

        /// <summary>
        /// Gets the length of the local scope.
        /// </summary>
        public int Length => LocalScope.Length;

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
