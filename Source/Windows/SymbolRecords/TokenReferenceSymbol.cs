using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents managed token reference symbol.
    /// </summary>
    public class TokenReferenceSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_TOKENREF,
        };

        /// <summary>
        /// Gets the checksum of the referenced symbol name. The checksum used is the
        /// one specified in the header of the global symbols stream or static symbols stream.
        /// </summary>
        public uint Checksum { get; private set; }

        /// <summary>
        /// Gets the offset of the procedure symbol record from the beginning of the
        /// $$SYMBOL table for the module.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Index of the module that contains this procedure record.
        /// </summary>
        public ushort Module { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the token parsed from the <see cref="Name"/>.
        /// </summary>
        public int Token => int.Parse(Name, System.Globalization.NumberStyles.HexNumber);

        /// <summary>
        /// Reads <see cref="TokenReferenceSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static TokenReferenceSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new TokenReferenceSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Checksum = reader.ReadUint(),
                Offset = reader.ReadUint(),
                Module = reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
