using SharpPdb.Native.Types;
using SharpPdb.Windows.SymbolRecords;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents constant static field of the type read from the PDB file.
    /// </summary>
    public class PdbTypeConstant : PdbTypeStaticField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbTypeConstant"/> class.
        /// </summary>
        /// <param name="containerType">Type that contains this field.</param>
        /// <param name="staticDataMemberRecord">The static data member record.</param>
        /// <param name="constant">The constant symbol.</param>
        internal PdbTypeConstant(PdbUserDefinedType containerType, StaticDataMemberRecord staticDataMemberRecord, ConstantSymbol constant)
            : base(containerType, staticDataMemberRecord)
        {
            Constant = constant;
        }

        /// <summary>
        /// Gets the constant symbol.
        /// </summary>
        public ConstantSymbol Constant { get; private set; }

        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        public object Value => Constant.Value;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"({Access}) {Type.Name} {Name} = {Value}";
        }
    }
}
