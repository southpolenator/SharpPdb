using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents frame procedure symbol from the symbols stream.
    /// </summary>
    public class FrameProcedureSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_FRAMEPROC,
        };

        /// <summary>
        /// Gets the total frame bytes.
        /// </summary>
        public uint TotalFrameBytes { get; private set; }

        /// <summary>
        /// Gets the padding frame bytes.
        /// </summary>
        public uint PaddingFrameBytes { get; private set; }

        /// <summary>
        /// Gets the offset to padding.
        /// </summary>
        public uint OffsetToPadding { get; private set; }

        /// <summary>
        /// Gets the number of bytes for callee saved registers.
        /// </summary>
        public uint BytesOfCalleeSavedRegisters { get; private set; }

        /// <summary>
        /// Gets the offset of exception handler.
        /// </summary>
        public uint OffsetOfExceptionHandler { get; private set; }

        /// <summary>
        /// Gets the section id of exception handler.
        /// </summary>
        public ushort SectionIdOfExceptionHandler { get; private set; }

        /// <summary>
        /// Gets the frame procedure flags.
        /// </summary>
        public FrameProcedureOptions Flags { get; private set; }

        /// <summary>
        /// Reads <see cref="FrameProcedureSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static FrameProcedureSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new FrameProcedureSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                TotalFrameBytes = reader.ReadUint(),
                PaddingFrameBytes = reader.ReadUint(),
                OffsetToPadding = reader.ReadUint(),
                BytesOfCalleeSavedRegisters = reader.ReadUint(),
                OffsetOfExceptionHandler = reader.ReadUint(),
                SectionIdOfExceptionHandler = reader.ReadUshort(),
                Flags = (FrameProcedureOptions)reader.ReadUint(),
            };
        }
    }
}
