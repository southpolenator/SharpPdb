using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents complex pointer type read from PDB file (type that has <see cref="PointerRecord"/> to better describe it).
    /// </summary>
    public class PdbComplexPointerType : PdbPointerType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbComplexPointerType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="pointerRecord">The pointer record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbComplexPointerType(PdbFileReader pdb, TypeIndex typeIndex, PointerRecord pointerRecord, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, CombineModifierOptions(modifierOptions, pointerRecord), pointerRecord.ReferentType, GetPointerSize(pointerRecord), pointerRecord.Mode == PointerMode.LValueReference, pointerRecord.Mode == PointerMode.RValueReference)
        {
            PointerRecord = pointerRecord;
        }

        /// <summary>
        /// Gets the pointer record.
        /// </summary>
        public PointerRecord PointerRecord { get; private set; }

        /// <summary>
        /// Combines modifier options with pointer record data.
        /// </summary>
        /// <param name="modifierOptions">The modifier options.</param>
        /// <param name="pointerRecord">The pointer record.</param>
        private static ModifierOptions CombineModifierOptions(ModifierOptions modifierOptions, PointerRecord pointerRecord)
        {
            if (pointerRecord.IsConst)
                modifierOptions |= ModifierOptions.Const;
            if (pointerRecord.IsVolatile)
                modifierOptions |= ModifierOptions.Volatile;
            if (pointerRecord.IsUnaligned)
                modifierOptions |= ModifierOptions.Unaligned;
            return modifierOptions;
        }

        /// <summary>
        /// Gets pointer size from pointer type record.
        /// </summary>
        /// <param name="pointerRecord">The pointer record.</param>
        private static ulong GetPointerSize(PointerRecord pointerRecord)
        {
            if (pointerRecord.Size != 0)
                return pointerRecord.Size;
            return pointerRecord.PointerKind == PointerKind.Near64 ? 8U : 4U;
        }
    }
}
