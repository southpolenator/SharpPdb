using SharpUtilities;
using System.Reflection.Metadata;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents local variable defined in local scope in Portable PDB file.
    /// </summary>
    public class PdbLocalVariable : IPdbLocalVariable
    {
        /// <summary>
        /// Cache of <see cref="LocalVariable"/> property.
        /// </summary>
        private SimpleCacheStruct<LocalVariable> localVariableCache;

        /// <summary>
        /// Cache of <see cref="Name"/> property.
        /// </summary>
        private SimpleCacheStruct<string> nameCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbLocalVariable"/> class.
        /// </summary>
        /// <param name="localScope">Local scope where this variable is defined.</param>
        /// <param name="handle">Our metadata reader handle.</param>
        public PdbLocalVariable(PdbLocalScope localScope, LocalVariableHandle handle)
        {
            LocalScope = localScope;
            localVariableCache = SimpleCache.CreateStruct(() => LocalScope.Function.PdbFile.Reader.GetLocalVariable(handle));
            nameCache = SimpleCache.CreateStruct(() => LocalScope.Function.PdbFile.Reader.GetString(LocalVariable.Name));
        }

        /// <summary>
        /// Gets the local scope where this contanst is defined.
        /// </summary>
        public PdbLocalScope LocalScope { get; private set; }

        /// <summary>
        /// Gets the local variable.
        /// </summary>
        public LocalVariable LocalVariable => localVariableCache.Value;

        /// <summary>
        /// Gets the index on the stack of the local variable.
        /// </summary>
        public int Index => LocalVariable.Index;

        /// <summary>
        /// Gets the name of the local variable.
        /// </summary>
        public string Name => nameCache.Value;
    }
}
