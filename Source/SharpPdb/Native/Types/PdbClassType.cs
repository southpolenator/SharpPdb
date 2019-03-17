using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents class, structure or interface type read from PDB file.
    /// </summary>
    public class PdbClassType : PdbUserDefinedType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="classRecord">The class record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbClassType(PdbFileReader pdb, TypeIndex typeIndex, ClassRecord classRecord, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, classRecord, modifierOptions, classRecord.Size)
        {
            ClassRecord = classRecord;
        }

        /// <summary>
        /// Gets the class record.
        /// </summary>
        public ClassRecord ClassRecord { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (ClassRecord.Kind == TypeLeafKind.LF_CLASS)
                return $"class {Name} ({Size})";
            if (ClassRecord.Kind == TypeLeafKind.LF_STRUCTURE)
                return $"struct {Name} ({Size})";
            if (ClassRecord.Kind == TypeLeafKind.LF_INTERFACE)
                return $"interface {Name} ({Size})";
            return base.ToString();
        }
    }
}
