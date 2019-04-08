using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents managed attribute slot symbol.
    /// </summary>
    public class AttributeSlotSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_MANSLOT,
        };

        /// <summary>
        /// Gets the slot index.
        /// </summary>
        public uint Index { get; private set; }

        /// <summary>
        /// Gets the type index or metadata token.
        /// </summary>
        public TypeIndex TypeIndex { get; private set; }

        /// <summary>
        /// Gets the first code address where variable is live.
        /// </summary>
        public uint CodeOffset { get; private set; }

        /// <summary>
        /// Gets the segment.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the local variable flags.
        /// </summary>
        public LocalVariableFlags Flags { get; private set; }

        /// <summary>
        /// Gets the variable name.
        /// </summary>
        public StringReference Name;

        /// <summary>
        /// Reads <see cref="AttributeSlotSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static AttributeSlotSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new AttributeSlotSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Index = reader.ReadUint(),
                TypeIndex = TypeIndex.Read(reader),
                CodeOffset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Flags = (LocalVariableFlags)reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
