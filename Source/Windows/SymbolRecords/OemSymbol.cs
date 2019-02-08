using SharpPdb.Windows.Utility;
using System;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents the OEM symbol. It contains lots of custom metadata from MSIL.
    /// </summary>
    public class OemSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_OEM,
        };

        /// <summary>
        /// Gets the OEM identifier.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the type index.
        /// </summary>
        public TypeIndex TypeIndex { get; private set; }

        /// <summary>
        /// Gets the user data binary reader.
        /// </summary>
        public IBinaryReader UserDataReader { get; private set; }

        /// <summary>
        /// Reads <see cref="OemSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static OemSymbol Read(IBinaryReader reader, SymbolRecordKind kind, uint dataLength)
        {
            long positionEnd = reader.Position + dataLength;
            OemSymbol symbol = new OemSymbol
            {
                Kind = kind,
                Id = reader.ReadGuid(),
                TypeIndex = TypeIndex.Read(reader),
            };

            reader.Align(4);
            symbol.UserDataReader = reader.ReadSubstream(positionEnd - reader.Position);
            return symbol;
        }
    }
}
