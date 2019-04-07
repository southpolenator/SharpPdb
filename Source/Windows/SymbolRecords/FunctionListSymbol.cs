using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents function list symbol from the symbols stream.
    /// </summary>
    public class FunctionListSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_CALLERS, SymbolRecordKind.S_CALLEES,
        };

        /// <summary>
        /// Gets the number of functions.
        /// </summary>
        public uint Count { get; private set; }

        /// <summary>
        /// Gets the functions.
        /// </summary>
        public TypeIndex[] Functions { get; private set; }

        /// <summary>
        /// Gets the function invocations.
        /// </summary>
        public uint[] Invocations { get; private set; }

        /// <summary>
        /// Reads <see cref="FunctionListSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static FunctionListSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind, uint dataLength)
        {
            long start = reader.Position;
            var symbol = new FunctionListSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Count = reader.ReadUint(),
            };

            symbol.Functions = new TypeIndex[symbol.Count];
            symbol.Invocations = new uint[symbol.Count];
            for (int i = 0; i < symbol.Functions.Length; i++)
                symbol.Functions[i] = TypeIndex.Read(reader);

            int remaining = (int)(dataLength - (reader.Position - start)) / 4; // 4 = sizeof(uint)

            if (remaining > symbol.Invocations.Length)
                remaining = symbol.Invocations.Length;
            for (int i = 0; i < remaining; i++)
                symbol.Invocations[i] = reader.ReadUint();

            return symbol;
        }
    }
}
