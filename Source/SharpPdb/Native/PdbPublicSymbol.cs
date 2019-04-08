using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents public symbol read from the PDB file.
    /// </summary>
    public class PdbPublicSymbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbPublicSymbol"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="publicSymbol">Public symbol record.</param>
        internal PdbPublicSymbol(PdbFileReader pdb, Public32Symbol publicSymbol)
        {
            Pdb = pdb;
            PublicSymbol = publicSymbol;
        }

        /// <summary>
        /// Gets the PDB File reader.
        /// </summary>
        public PdbFileReader Pdb { get; private set; }

        /// <summary>
        /// Gets the public symbol record.
        /// </summary>
        public Public32Symbol PublicSymbol { get; private set; }

        /// <summary>
        /// Gets public symbol flags.
        /// </summary>
        public PublicSymbolFlags Flags => PublicSymbol.Flags;

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public uint Offset => PublicSymbol.Offset;

        /// <summary>
        /// Gets the segment.
        /// </summary>
        public ushort Segment => PublicSymbol.Segment;

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => PublicSymbol.Name.String;

        /// <summary>
        /// Gets the relative virtual address of this static field from module load address.
        /// </summary>
        public ulong RelativeVirtualAddress => Pdb.PdbFile.FindRelativeVirtualAddress(Segment, Offset);

        /// <summary>
        /// <c>true</c> if the symbol's location is in code.
        /// </summary>
        public bool IsCode => (Flags & (PublicSymbolFlags.Code | PublicSymbolFlags.Function)) != 0;

        /// <summary>
        /// <c>true</c> if the symbol is a function.
        /// </summary>
        public bool IsFunction => (Flags & PublicSymbolFlags.Function) != 0;

        /// <summary>
        /// <c>true</c> if the symbol's location is in managed code.
        /// </summary>
        public bool IsManaged => (Flags & PublicSymbolFlags.Managed) != 0;

        /// <summary>
        /// <c>true</c> if the symbol's location is in Microsoft Intermediate Language (MSIL) code.
        /// </summary>
        public bool IsMsil => (Flags & PublicSymbolFlags.MSIL) != 0;

        /// <summary>
        /// Gets the part or all of the undecorated symbol name.
        /// </summary>
        /// <param name="flags">Specifies a combination of flags that control what is returned.</param>
        /// <returns>Returns the undecorated name for a C++ decorated name.</returns>
        public string GetUndecoratedName(NameUndecorator.Flags flags = NameUndecorator.Flags.Complete)
        {
            return NameUndecorator.UnDecorateSymbolName(Name, flags);
        }
    }
}
