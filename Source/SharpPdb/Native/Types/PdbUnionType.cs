using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents union type read from PDB file.
    /// </summary>
    public class PdbUnionType : PdbUserDefinedType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="unionRecord">The union record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbUnionType(PdbFileReader pdb, TypeIndex typeIndex, UnionRecord unionRecord, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, unionRecord, modifierOptions, unionRecord.Size)
        {
            UnionRecord = unionRecord;
        }

        /// <summary>
        /// Gets the union record.
        /// </summary>
        public UnionRecord UnionRecord { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"union {Name} ({Size})";
        }
    }
}
