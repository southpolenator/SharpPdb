using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents pointer type read from PDB file.
    /// </summary>
    public class PdbPointerType : PdbType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbPointerType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        /// <param name="elementType">PDB type of object pointed to.</param>
        /// <param name="size">The type size in bytes.</param>
        /// <param name="isLValueReference"><c>true</c> if pointer is L-value reference</param>
        /// <param name="isRValueReference"><c>true</c> if pointer is R-value reference</param>
        internal PdbPointerType(PdbFileReader pdb, TypeIndex typeIndex, ModifierOptions modifierOptions, TypeIndex elementType, ulong size, bool isLValueReference, bool isRValueReference)
            : base(pdb, typeIndex, modifierOptions, pdb[elementType].Name + "*", size)
        {
            ElementType = pdb[elementType];
            IsLValueReference = isLValueReference;
            IsRValueReference = isRValueReference;
        }

        /// <summary>
        /// Gets the PDB type of object pointed to.
        /// </summary>
        public PdbType ElementType { get; private set; }

        /// <summary>
        /// <c>true</c> if pointer is L-value reference
        /// </summary>
        public bool IsLValueReference { get; private set; }

        /// <summary>
        /// <c>true</c> if pointer is R-value reference
        /// </summary>
        public bool IsRValueReference { get; private set; }
    }
}
