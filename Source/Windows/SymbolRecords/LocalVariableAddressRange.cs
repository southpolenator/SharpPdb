using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents local variable address range.
    /// </summary>
    public struct LocalVariableAddressRange
    {
        /// <summary>
        /// Size of <see cref="LocalVariableAddressGap"/> structure in bytes.
        /// </summary>
        public const int Size = 8;

        /// <summary>
        /// Gets the offset start.
        /// </summary>
        public uint OffsetStart { get; private set; }

        /// <summary>
        /// Gets the section start.
        /// </summary>
        public ushort ISectStart { get; private set; }

        /// <summary>
        /// Gets the range.
        /// </summary>
        public ushort Range { get; private set; }

        /// <summary>
        /// Reads <see cref="LocalVariableAddressRange"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static LocalVariableAddressRange Read(IBinaryReader reader)
        {
            return new LocalVariableAddressRange
            {
                OffsetStart = reader.ReadUint(),
                ISectStart = reader.ReadUshort(),
                Range = reader.ReadUshort(),
            };
        }
    }
}
