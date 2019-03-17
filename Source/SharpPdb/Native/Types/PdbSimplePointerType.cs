using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;
using System;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents simple pointer type read from PDB file (type that is described with <see cref="TypeIndex"/>).
    /// </summary>
    public class PdbSimplePointerType : PdbPointerType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSimplePointerType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbSimplePointerType(PdbFileReader pdb, TypeIndex typeIndex, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, modifierOptions, new TypeIndex(typeIndex.SimpleKind), GetPointerSize(typeIndex), isLValueReference: false, isRValueReference: false)
        {
        }

        /// <summary>
        /// Gets the pointer size from type index.
        /// </summary>
        /// <param name="typeIndex">The type index.</param>
        private static ulong GetPointerSize(TypeIndex typeIndex)
        {
            switch (typeIndex.SimpleMode)
            {
                case SimpleTypeMode.Direct:
                    return 0;
                case SimpleTypeMode.NearPointer:
                    return 2;
                case SimpleTypeMode.FarPointer:
                case SimpleTypeMode.HugePointer:
                case SimpleTypeMode.FarPointer32:
                case SimpleTypeMode.NearPointer32:
                    return 4;
                case SimpleTypeMode.NearPointer64:
                case SimpleTypeMode.NearPointer128:
                    return 8;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
