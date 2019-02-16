using SharpPdb.Windows.SymbolRecords;

namespace SharpPdb.Managed.Windows
{
    /// <summary>
    /// Represents local variable defined in local scope in Windows PDB file.
    /// </summary>
    public class PdbLocalVariable : IPdbLocalVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbLocalVariable"/> class.
        /// </summary>
        /// <param name="localScope">Local scope where this variable is defined.</param>
        /// <param name="slot">Attribute slot symbol read from PDB.</param>
        internal PdbLocalVariable(PdbLocalScope localScope, AttributeSlotSymbol slot)
        {
            LocalScope = localScope;
            Slot = slot;
        }

        /// <summary>
        /// Gets the local scope where this contanst is defined.
        /// </summary>
        public PdbLocalScope LocalScope { get; private set; }

        /// <summary>
        /// Gets the attribute slot symbol.
        /// </summary>
        public AttributeSlotSymbol Slot { get; private set; }

        /// <summary>
        /// Gets the index on the stack of the local variable.
        /// </summary>
        public int Index => (int)Slot.Index;

        /// <summary>
        /// Gets the name of the local variable.
        /// </summary>
        public string Name => Slot.Name;
    }
}
