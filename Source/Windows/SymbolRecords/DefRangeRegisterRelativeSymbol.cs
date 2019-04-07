using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents defrange register relative symbol from the symbols stream.
    /// </summary>
    public class DefRangeRegisterRelativeSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_DEFRANGE_REGISTER_REL,
        };

        /// <summary>
        /// Gets the register id.
        /// </summary>
        public RegisterId Register { get; private set; }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        public ushort Flags { get; private set; }

        /// <summary>
        /// Gets the base pointer offset.
        /// </summary>
        public int BasePointerOffset { get; private set; }

        /// <summary>
        /// Gets the local variable address range.
        /// </summary>
        public LocalVariableAddressRange Range { get; private set; }

        /// <summary>
        /// Gets the array of local variable address gaps.
        /// </summary>
        public LocalVariableAddressGap[] Gaps { get; private set; }

        /// <summary>
        /// Reads <see cref="DefRangeRegisterRelativeSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static DefRangeRegisterRelativeSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind, uint dataLength)
        {
            long start = reader.Position;
            var symbol = new DefRangeRegisterRelativeSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Register = (RegisterId)reader.ReadUshort(),
                Flags = reader.ReadUshort(),
                BasePointerOffset = reader.ReadInt(),
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
