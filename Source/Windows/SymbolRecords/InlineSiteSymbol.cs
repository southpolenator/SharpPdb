using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents inline site symbol record.
    /// </summary>
    public class InlineSiteSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_INLINESITE
        };

        /// <summary>
        /// Used in local procedures, global procedures, thunk start, with start, and
        /// block start symbols. If the scope is not enclosed by another lexical scope,
        /// then <see cref="ParentOffset"/> is zero. Otherwise, the parent of this scope is the symbol
        /// within this module that opens the outer scope that encloses this scope but
        /// encloses no other scope that encloses this scope. The <see cref="ParentOffset"/> field contains
        /// the offset from the beginning of the module's symbol table of the symbol
        /// that opens the enclosing lexical scope.
        /// </summary>
        public uint ParentOffset { get; private set; }

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
        /// Gets the inlinee type index.
        /// </summary>
        public TypeIndex Inlinee { get; private set; }

        /// <summary>
        /// Gets the annotation data.
        /// </summary>
        public byte[] AnnotationData { get; private set; }

        /// <summary>
        /// Reads <see cref="InlineSiteSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static InlineSiteSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind, uint dataLength)
        {
            long start = reader.Position;
            return new InlineSiteSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                ParentOffset = reader.ReadUint(),
                End = reader.ReadUint(),
                Inlinee = TypeIndex.Read(reader),
                AnnotationData = reader.ReadByteArray((int)(dataLength - (reader.Position - start))),
            };
        }

        /// <summary>
        /// Gets end position of the children subrecords.
        /// </summary>
        /// <returns>Position in the binary stream.</returns>
        protected override long GetChildrenEndPosition()
        {
            return End;
        }

        /// <summary>
        /// Gets the position in the symbol stream of the parent symbol record.
        /// </summary>
        /// <returns>Position in the symbol stream of the parent symbol record.</returns>
        protected override long GetParentPosition()
        {
            return ParentOffset;
        }
    }
}
