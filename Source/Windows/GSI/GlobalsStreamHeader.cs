using SharpUtilities;

namespace SharpPdb.Windows.GSI
{
    /// <summary>
    /// Globals stream header structure.
    /// </summary>
    public struct GlobalsStreamHeader
    {
        /// <summary>
        /// Size of <see cref="GlobalsStreamHeader"/> structure in bytes.
        /// </summary>
        public const int Size = 16;

        /// <summary>
        /// Extected globals stream header signature.
        /// </summary>
        public const uint ExpectedSignature = 0xFFFFFFFF;

        /// <summary>
        /// Expected globals stream header version;
        /// </summary>
        public const uint ExpectedVersion = 0xeffe0000 + 19990810;

        /// <summary>
        /// Gets the header signature.
        /// </summary>
        public uint Signature { get; private set; }

        /// <summary>
        /// Gets the stream version.
        /// </summary>
        public uint Version { get; private set; }

        /// <summary>
        /// Gets the hash records substream size.
        /// </summary>
        public uint HashRecordsSubstreamSize { get; private set; }

        /// <summary>
        /// Gets the hash buckets substream size.
        /// </summary>
        public uint HashBucketsSubstreamSize { get; private set; }

        /// <summary>
        /// Reads <see cref="GlobalsStreamHeader"/> from the stream.
        /// </summary>
        /// <param name="reader">Strem binary reader.</param>
        public static GlobalsStreamHeader Read(IBinaryReader reader)
        {
            return new GlobalsStreamHeader
            {
                Signature = reader.ReadUint(),
                Version = reader.ReadUint(),
                HashRecordsSubstreamSize = reader.ReadUint(),
                HashBucketsSubstreamSize = reader.ReadUint(),
            };
        }
    }
}
