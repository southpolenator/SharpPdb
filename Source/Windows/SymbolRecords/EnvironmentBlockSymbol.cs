using SharpUtilities;
using System.Collections.Generic;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents environment block symbol from the symbols stream.
    /// </summary>
    public class EnvironmentBlockSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_ENVBLOCK,
        };

        /// <summary>
        /// List of fields of this environment block.
        /// It should be dictionary (key => value) for consecutive entries: key1, value1, key2, value2, ...
        /// </summary>
        public IReadOnlyList<string> Fields { get; private set; }

        /// <summary>
        /// Reads <see cref="EnvironmentBlockSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static EnvironmentBlockSymbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            var result = new EnvironmentBlockSymbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
            };
            byte reserved = reader.ReadByte();
            List<string> fields = new List<string>();

            for (string field = reader.ReadCString().String; !string.IsNullOrEmpty(field); field = reader.ReadCString().String)
                fields.Add(field);
            result.Fields = fields;
            return result;
        }
    }
}
