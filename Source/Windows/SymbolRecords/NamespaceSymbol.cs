using SharpPdb.Windows.Utility;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents namespace symbol.
    /// </summary>
    public class NamespaceSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_UNAMESPACE,
        };

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Reads <see cref="NamespaceSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static NamespaceSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new NamespaceSymbol
            {
                Kind = kind,
                Namespace = reader.ReadCString(),
            };
        }
    }
}
