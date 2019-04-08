using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents file static symbol from the symbols stream.
    /// </summary>
    public class FileStaticSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_FILESTATIC,
        };

        /// <summary>
        /// Gets the symbol type.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the module filename offset.
        /// </summary>
        public uint ModFilenameOffset { get; private set; }

        /// <summary>
        /// Gets the variable flags.
        /// </summary>
        public LocalVariableFlags Flags { get; private set; }

        /// <summary>
        /// Gets the variable name.
        /// </summary>
        public StringReference Name;

        /// <summary>
        /// Reads <see cref="FileStaticSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static FileStaticSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new FileStaticSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Type = TypeIndex.Read(reader),
                ModFilenameOffset = reader.ReadUint(),
                Flags = (LocalVariableFlags)reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
