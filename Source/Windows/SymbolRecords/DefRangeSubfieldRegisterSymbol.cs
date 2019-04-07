using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents defrange subfield register symbol from the symbols stream.
    /// </summary>
    public class DefRangeSubfieldRegisterSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_DEFRANGE_SUBFIELD_REGISTER,
        };

        /// <summary>
        /// Gets the register id.
        /// </summary>
        public RegisterId Register { get; private set; }

        /// <summary>
        /// Gets the flag representing if this symbol may have no name.
        /// </summary>
        public ushort MayHaveNoName { get; private set; }

        /// <summary>
        /// Gets the offset in parent.
        /// </summary>
        public uint OffsetInParent { get; private set; }

        /// <summary>
        /// Gets the local variable address range.
        /// </summary>
        public LocalVariableAddressRange Range { get; private set; }

        /// <summary>
        /// Gets the array of local variable address gaps.
        /// </summary>
        public LocalVariableAddressGap[] Gaps { get; private set; }

        /// <summary>
        /// Reads <see cref="DefRangeSubfieldRegisterSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static DefRangeSubfieldRegisterSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind, uint dataLength)
        {
            long start = reader.Position;
            var symbol = new DefRangeSubfieldRegisterSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Register = (RegisterId)reader.ReadUshort(),
                MayHaveNoName = reader.ReadUshort(),
                OffsetInParent = reader.ReadUint(),
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
