using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents heap allocation site symbol from the symbols stream.
    /// </summary>
    public class HeapAllocationSiteSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_HEAPALLOCSITE,
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
        /// Gets the call instruction size.
        /// </summary>
        public ushort CallInstructionSize { get; private set; }

        /// <summary>
        /// Gets the symbol type.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Reads <see cref="HeapAllocationSiteSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static HeapAllocationSiteSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new HeapAllocationSiteSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                CallInstructionSize = reader.ReadUshort(),
                Type = TypeIndex.Read(reader),
            };
        }
    }
}
