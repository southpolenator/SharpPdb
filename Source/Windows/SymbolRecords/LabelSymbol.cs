using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents label symbol from the symbols stream.
    /// </summary>
    public class LabelSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_LABEL32,
        };

        /// <summary>
        /// Gets the offset portion of symbol address.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the segment portion of symbol address.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the label flags.
        /// </summary>
        public ProcedureFlags Flags { get; private set; }

        /// <summary>
        /// Gets the label name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="LabelSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static LabelSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new LabelSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Flags = (ProcedureFlags)reader.ReadByte(),
                Name = reader.ReadCString(),
            };
        }
    }
}
