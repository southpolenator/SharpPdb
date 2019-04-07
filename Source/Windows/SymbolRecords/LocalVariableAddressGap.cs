using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents local variable address gap.
    /// </summary>
    public struct LocalVariableAddressGap
    {
        /// <summary>
        /// Size of <see cref="LocalVariableAddressGap"/> structure in bytes.
        /// </summary>
        public const int Size = 4;

        /// <summary>
        /// Gets the gap start offset.
        /// </summary>
        public ushort GapStartOffset { get; private set; }

        /// <summary>
        /// Gets the range.
        /// </summary>
        public ushort Range { get; private set; }

        /// <summary>
        /// Reads <see cref="LocalVariableAddressGap"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static LocalVariableAddressGap Read(IBinaryReader reader)
        {
            return new LocalVariableAddressGap
            {
                GapStartOffset = reader.ReadUshort(),
                Range = reader.ReadUshort(),
            };
        }
    }
}
