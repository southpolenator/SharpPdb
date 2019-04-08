using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// This symbol is inserted into the global and static symbol tables to reference a procedure. It is
    /// used so that the symbol procedure can be found in the hashed search of the global or static
    /// symbol table. Otherwise, procedures could be found only by searching the symbol table for
    /// every module.
    /// </summary>
    public class ProcedureReferenceSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_PROCREF, SymbolRecordKind.S_LPROCREF
        };

        /// <summary>
        /// Gets the checksum of the referenced symbol name. The checksum used is the
        /// one specified in the header of the global symbols stream or static symbols stream.
        /// </summary>
        public uint Checksum { get; private set; }

        /// <summary>
        /// Gets the offset of the procedure symbol record from the beginning of the
        /// $$SYMBOL table for the module.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Index of the module that contains this procedure record.
        /// </summary>
        public ushort Module { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public StringReference Name;

        /// <summary>
        /// Reads <see cref="ProcedureReferenceSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static ProcedureReferenceSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new ProcedureReferenceSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Checksum = reader.ReadUint(),
                Offset = reader.ReadUint(),
                Module = reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
