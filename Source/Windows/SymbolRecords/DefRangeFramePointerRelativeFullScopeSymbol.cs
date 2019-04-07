using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents defrange frame pointer relative full scope symbol from the symbols stream.
    /// </summary>
    public class DefRangeFramePointerRelativeFullScopeSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_DEFRANGE_FRAMEPOINTER_REL_FULL_SCOPE,
        };

        /// <summary>
        /// Gets the offset from frame pointer.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Reads <see cref="DefRangeFramePointerRelativeFullScopeSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static DefRangeFramePointerRelativeFullScopeSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new DefRangeFramePointerRelativeFullScopeSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Offset = reader.ReadUint(),
            };
        }
    }
}
