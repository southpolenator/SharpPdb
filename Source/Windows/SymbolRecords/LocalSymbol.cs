using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents local symbol from the symbols stream.
    /// </summary>
    public class LocalSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_LOCAL,
        };

        /// <summary>
        /// Gets the symbol type.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the local symbol flags.
        /// </summary>
        public LocalVariableFlags Flags { get; private set; }

        /// <summary>
        /// Gets the local symbol name.
        /// </summary>
        public StringReference Name;

        /// <summary>
        /// Reads <see cref="LocalSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static LocalSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new LocalSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Type = TypeIndex.Read(reader),
                Flags = (LocalVariableFlags)reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
