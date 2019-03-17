using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents annotation reference symbol.
    /// </summary>
    public class AnnotationReferenceSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_ANNOTATIONREF,
        };

        /// <summary>
        /// TODO: What's this?
        /// </summary>
        public uint SumName { get; private set; }

        /// <summary>
        /// Gets the symbol index.
        /// </summary>
        public uint SymbolIndex { get; private set; }

        /// <summary>
        /// Gets index of the module.
        /// </summary>
        public ushort Module { get; private set; }

        /// <summary>
        /// Reads <see cref="AnnotationReferenceSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static AnnotationReferenceSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new AnnotationReferenceSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                SumName = reader.ReadUint(),
                SymbolIndex = reader.ReadUint(),
                Module = reader.ReadUshort(),
            };
        }
    }
}
