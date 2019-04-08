using SharpPdb.Windows.SymbolRecords;

namespace SharpPdb.Managed.Windows
{
    /// <summary>
    /// Represents local constant defined in local scope in Windows PDB file.
    /// </summary>
    public class PdbLocalConstant : IPdbLocalConstant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbLocalConstant"/> class.
        /// </summary>
        /// <param name="localScope">Local scope where this contanst is defined.</param>
        /// <param name="constant">Constant symbol read from PDB.</param>
        internal PdbLocalConstant(PdbLocalScope localScope, ConstantSymbol constant)
        {
            LocalScope = localScope;
            Constant = constant;
        }

        /// <summary>
        /// Gets the local scope where this contanst is defined.
        /// </summary>
        public PdbLocalScope LocalScope { get; private set; }

        /// <summary>
        /// Gets the constant symbol read from PDB.
        /// </summary>
        public ConstantSymbol Constant { get; private set; }

        /// <summary>
        /// Gets the name of the local constant.
        /// </summary>
        public string Name => Constant.Name.String;

        /// <summary>
        /// Gets the value of the local constant.
        /// </summary>
        public object Value => Constant.Value;
    }
}
