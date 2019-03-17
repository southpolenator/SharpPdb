using SharpUtilities;

namespace SharpPdb.Windows.GSI
{
    /// <summary>
    /// Globals stream hash record structure.
    /// </summary>
    public struct GlobalsStreamHashRecord
    {
        /// <summary>
        /// Size of <see cref="GlobalsStreamHashRecord"/> structure in bytes.
        /// </summary>
        public const int Size = 8;

        /// <summary>
        /// Gets the offset in symbols stream.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the reference count.
        /// </summary>
        public uint ReferenceCount { get; private set; }

        /// <summary>
        /// Reads <see cref="GlobalsStreamHashRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Strem binary reader.</param>
        public static GlobalsStreamHashRecord Read(IBinaryReader reader)
        {
            return new GlobalsStreamHashRecord
            {
                Offset = reader.ReadUint(),
                ReferenceCount = reader.ReadUint(),
            };
        }
    }
}
