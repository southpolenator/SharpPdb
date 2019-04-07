using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents frame cookie symbol from the symbols stream.
    /// </summary>
    public class FrameCookieSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_FRAMECOOKIE,
        };

        /// <summary>
        /// Gets the code offset.
        /// </summary>
        public uint CodeOffset { get; private set; }

        /// <summary>
        /// Gets the register id.
        /// </summary>
        public RegisterId Register { get; private set; }

        /// <summary>
        /// Gets the frame cookie kind.
        /// </summary>
        public FrameCookieKind CookieKind { get; private set; }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        public byte Flags { get; private set; }

        /// <summary>
        /// Reads <see cref="FrameCookieSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static FrameCookieSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new FrameCookieSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                CodeOffset = reader.ReadUint(),
                Register = (RegisterId)reader.ReadUshort(),
                CookieKind = (FrameCookieKind)reader.ReadByte(),
                Flags = reader.ReadByte(),
            };
        }
    }
}
