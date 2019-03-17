using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents array type read from PDB file.
    /// </summary>
    public class PdbArrayType : PdbType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbArrayType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="arrayRecord">The array record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbArrayType(PdbFileReader pdb, TypeIndex typeIndex, ArrayRecord arrayRecord, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, modifierOptions, GetTypeName(pdb, arrayRecord), arrayRecord.Size)
        {
            ArrayRecord = arrayRecord;
        }

        /// <summary>
        /// Gets the array record.
        /// </summary>
        public ArrayRecord ArrayRecord { get; private set; }

        /// <summary>
        /// Gets the PDB type of indexing variable.
        /// </summary>
        public PdbType IndexType => Pdb[ArrayRecord.IndexType];

        /// <summary>
        /// Gets the PDB type of each array element.
        /// </summary>
        public PdbType ElementType => Pdb[ArrayRecord.ElementType];

        /// <summary>
        /// Gets the number of elements in the array.
        /// </summary>
        public ulong Count => ElementType.Size != 0 ? Size / ElementType.Size : 0;

        /// <summary>
        /// Gets the type name from array record.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="arrayRecord">The array record.</param>
        private static string GetTypeName(PdbFileReader pdb, ArrayRecord arrayRecord)
        {
            PdbType elementType = pdb[arrayRecord.ElementType];

            if (elementType.Size == 0)
                return $"{elementType.Name}[]";

            ulong elements = arrayRecord.Size / elementType.Size;

            return $"{elementType.Name}[{elements}]";
        }
    }
}
