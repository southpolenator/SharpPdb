using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents defrange frame pointer relative symbol from the symbols stream.
    /// </summary>
    public class DefRangeFramePointerRelativeSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_DEFRANGE_FRAMEPOINTER_REL,
        };

        /// <summary>
        /// Gets the offset from frame pointer.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the local variable address range.
        /// </summary>
        public LocalVariableAddressRange Range { get; private set; }

        /// <summary>
        /// Gets the array of local variable address gaps.
        /// </summary>
        public LocalVariableAddressGap[] Gaps { get; private set; }

        /// <summary>
        /// Reads <see cref="DefRangeFramePointerRelativeSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static DefRangeFramePointerRelativeSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind, uint dataLength)
        {
            long start = reader.Position;
            var symbol = new DefRangeFramePointerRelativeSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Offset = reader.ReadUint(),
                Range = LocalVariableAddressRange.Read(reader),
            };

            int count = (int)(dataLength - (reader.Position - start)) / LocalVariableAddressGap.Size;
            symbol.Gaps = new LocalVariableAddressGap[count];
            for (int i = 0; i < count; i++)
                symbol.Gaps[i] = LocalVariableAddressGap.Read(reader);
            return symbol;
        }
    }
}
