using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents type read from PDB file.
    /// </summary>
    public abstract class PdbType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        /// <param name="name">The type name.</param>
        /// <param name="size">The type size in bytes.</param>
        internal PdbType(PdbFileReader pdb, TypeIndex typeIndex, ModifierOptions modifierOptions, string name, ulong size)
        {
            Pdb = pdb;
            ModifierOptions = modifierOptions;
            TypeIndex = typeIndex;
            Name = name;
            Size = size;
        }

        /// <summary>
        /// Gets the PDB File reader.
        /// </summary>
        public PdbFileReader Pdb { get; private set; }

        /// <summary>
        /// Gets the modifier options.
        /// </summary>
        public ModifierOptions ModifierOptions { get; private set; }

        /// <summary>
        /// Gets the type name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type size in bytes.
        /// </summary>
        public ulong Size { get; private set; }

        /// <summary>
        /// Gets the type index.
        /// </summary>
        public TypeIndex TypeIndex { get; private set; }
    }
}
