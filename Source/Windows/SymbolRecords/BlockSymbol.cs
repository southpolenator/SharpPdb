using SharpPdb.Windows.Utility;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents code block symbol.
    /// </summary>
    public class BlockSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_BLOCK32,
        };

        /// <summary>
        /// Used in local procedures, global procedures, thunk start, with start, and
        /// block start symbols. If the scope is not enclosed by another lexical scope,
        /// then <see cref="Parent"/> is zero. Otherwise, the parent of this scope is the symbol
        /// within this module that opens the outer scope that encloses this scope but
        /// encloses no other scope that encloses this scope. The <see cref="Parent"/> field contains
        /// the offset from the beginning of the module's symbol table of the symbol
        /// that opens the enclosing lexical scope.
        /// </summary>
        public uint Parent { get; private set; }

        /// <summary>
        /// Used in start search local procedures, global procedures, and thunk start
        /// symbols. For each segment or section
        /// represented in the symbol table, there is a start search symbol that contains
        /// the offset from the start of the symbols for this module to the first procedure
        /// or thunk contained in the segment. Each outermost lexical scope symbol
        /// has a next field containing the next outermost scope symbol contained in the
        /// segment. The last outermost scope in the symbol table for each segment has
        /// a next field of zero.
        /// </summary>
        public uint End { get; private set; }

        /// <summary>
        /// Gets the length in bytes of this procedure.
        /// </summary>
        public uint CodeSize { get; private set; }

        /// <summary>
        /// Gets the offset portion of the procedure address.
        /// </summary>
        public uint CodeOffset { get; private set; }

        /// <summary>
        /// Gets the segment portion of the procedure address.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the name of procedure.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="BlockSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static BlockSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new BlockSymbol
            {
                Kind = kind,
                Parent = reader.ReadUint(),
                End = reader.ReadUint(),
                CodeSize = reader.ReadUint(),
                CodeOffset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
