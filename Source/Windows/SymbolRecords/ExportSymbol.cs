using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents export symbol record.
    /// </summary>
    public class ExportSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_EXPORT
        };

        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        public ushort Ordinal { get; private set; }

        /// <summary>
        /// Gets the export symbol flags.
        /// </summary>
        public ExportFlags Flags { get; private set; }

        /// <summary>
        /// Gets the export symbol name.
        /// </summary>
        public StringReference Name;

        /// <summary>
        /// Reads <see cref="ExportSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static ExportSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new ExportSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Ordinal = reader.ReadUshort(),
                Flags = (ExportFlags)reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
