using SharpPdb.Windows.SymbolRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents global variable read from the PDB file.
    /// </summary>
    public class PdbGlobalVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbTypeRegularStaticField"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="data">Data symbol for this static field.</param>
        internal PdbGlobalVariable(PdbFileReader pdb, DataSymbol data)
        {
            Pdb = pdb;
            Data = data;
        }

        /// <summary>
        /// Gets the PDB File reader.
        /// </summary>
        public PdbFileReader Pdb { get; private set; }

        /// <summary>
        /// Gets the data symbol for this static field.
        /// </summary>
        public DataSymbol Data { get; private set; }

        /// <summary>
        /// Gets the global variable name.
        /// </summary>
        public string Name => Data.Name;

        /// <summary>
        /// Gets the global variable type.
        /// </summary>
        public PdbType Type => Pdb[Data.Type];

        /// <summary>
        /// Gets the offset portion of symbol address.
        /// </summary>
        public uint Offset => Data.Offset;

        /// <summary>
        /// Gets the segment portion of symbol address.
        /// </summary>
        public ushort Segment => Data.Segment;

        /// <summary>
        /// Gets the relative virtual address of this static field from module load address.
        /// </summary>
        public ulong RelativeVirtualAddress => Pdb.PdbFile.FindRelativeVirtualAddress(Segment, Offset);
    }
}
