using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents annotation symbol from the symbols stream.
    /// </summary>
    public class AnnotationSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_ANNOTATION,
        };

        /// <summary>
        /// Gets the offset portion of symbol address.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the segment portion of symbol address.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the number of annotations.
        /// </summary>
        public ushort AnnotationsCount { get; private set; }

        /// <summary>
        /// Gets the annotations.
        /// </summary>
        public StringReference[] Annotations { get; private set; }

        /// <summary>
        /// Reads <see cref="AnnotationSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static AnnotationSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            var annotationSymbol = new AnnotationSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                AnnotationsCount = reader.ReadUshort(),
            };

            annotationSymbol.Annotations = new StringReference[annotationSymbol.AnnotationsCount];
            for (int i = 0; i < annotationSymbol.Annotations.Length; i++)
                annotationSymbol.Annotations[i] = reader.ReadCString();
            return annotationSymbol;
        }
    }
}
