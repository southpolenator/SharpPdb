using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;
using System;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents simple type read from PDB file (type that is described with <see cref="TypeIndex"/>).
    /// </summary>
    public class PdbSimpleType : PdbType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSimpleType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbSimpleType(PdbFileReader pdb, TypeIndex typeIndex, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, modifierOptions, typeIndex.SimpleTypeName, GetSize(typeIndex))
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets the type size from the type index.
        /// </summary>
        /// <param name="typeIndex">The type index.</param>
        private static ulong GetSize(TypeIndex typeIndex)
        {
            switch (typeIndex.SimpleKind)
            {
                case SimpleTypeKind.Void:
                case SimpleTypeKind.None:
                case SimpleTypeKind.NotTranslated:
                    return 0;
                case SimpleTypeKind.HResult:
                    return 4;
                case SimpleTypeKind.UnsignedCharacter:
                case SimpleTypeKind.NarrowCharacter:
                case SimpleTypeKind.SignedCharacter:
                    return 1;
                case SimpleTypeKind.Character16:
                case SimpleTypeKind.WideCharacter:
                    return 2;
                case SimpleTypeKind.Character32:
                    return 4;
                case SimpleTypeKind.Byte:
                case SimpleTypeKind.SByte:
                    return 1;
                case SimpleTypeKind.Int16Short:
                case SimpleTypeKind.Int16:
                case SimpleTypeKind.UInt16:
                case SimpleTypeKind.UInt16Short:
                    return 2;
                case SimpleTypeKind.Int32Long:
                case SimpleTypeKind.Int32:
                case SimpleTypeKind.UInt32Long:
                case SimpleTypeKind.UInt32:
                    return 4;
                case SimpleTypeKind.Int64Quad:
                case SimpleTypeKind.Int64:
                case SimpleTypeKind.UInt64Quad:
                case SimpleTypeKind.UInt64:
                    return 8;
                case SimpleTypeKind.Int128Oct:
                case SimpleTypeKind.Int128:
                case SimpleTypeKind.UInt128Oct:
                case SimpleTypeKind.UInt128:
                    return 16;
                case SimpleTypeKind.Float16:
                    return 2;
                case SimpleTypeKind.Float32:
                case SimpleTypeKind.Float32PartialPrecision:
                    return 4;
                case SimpleTypeKind.Float48:
                    return 6;
                case SimpleTypeKind.Float64:
                    return 8;
                case SimpleTypeKind.Float80:
                    return 10;
                case SimpleTypeKind.Float128:
                    return 16;
                case SimpleTypeKind.Complex16:
                    return 2;
                case SimpleTypeKind.Complex32:
                case SimpleTypeKind.Complex32PartialPrecision:
                    return 4;
                case SimpleTypeKind.Complex48:
                    return 6;
                case SimpleTypeKind.Complex64:
                    return 8;
                case SimpleTypeKind.Complex80:
                    return 10;
                case SimpleTypeKind.Complex128:
                    return 16;
                case SimpleTypeKind.Boolean8:
                    return 1;
                case SimpleTypeKind.Boolean16:
                    return 2;
                case SimpleTypeKind.Boolean32:
                    return 4;
                case SimpleTypeKind.Boolean64:
                    return 8;
                case SimpleTypeKind.Boolean128:
                    return 16;
                default:
                    throw new NotImplementedException($"Unexpected simple type: {typeIndex.SimpleKind}, from type index: {typeIndex}");
            }
        }
    }
}
