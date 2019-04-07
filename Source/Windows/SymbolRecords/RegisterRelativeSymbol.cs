using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents register relative symbol from the symbols stream.
    /// </summary>
    public class RegisterRelativeSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_REGREL32,
        };

        /// <summary>
        /// Gets the offset from the register value.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the symbol type.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the register id.
        /// </summary>
        public RegisterId Register { get; private set; }

        /// <summary>
        /// Gets the symbol name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="RegisterRelativeSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static RegisterRelativeSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new RegisterRelativeSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Offset = reader.ReadUint(),
                Type = TypeIndex.Read(reader),
                Register = (RegisterId)reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
