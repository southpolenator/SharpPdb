using SharpUtilities;

namespace SharpPdb.Windows.DBI
{
    /// <summary>
    /// Represents element from <see cref="KnownDebugStreamIndex.OmapFromSrc"/> or <see cref="KnownDebugStreamIndex.OmapToSrc"/> stream.
    /// </summary>
    public struct OmapEntry
    {
        /// <summary>
        /// Number of bytes needed to store this structure.
        /// </summary>
        public const int Size = 8;

        /// <summary>
        /// Original address that should be translated.
        /// </summary>
        public uint From { get; private set; }

        /// <summary>
        /// Resulting address that original address should be translated into.
        /// </summary>
        public uint To { get; private set; }

        /// <summary>
        /// Reads <see cref="OmapEntry"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static OmapEntry Read(IBinaryReader reader)
        {
            return new OmapEntry
            {
                From = reader.ReadUint(),
                To = reader.ReadUint(),
            };
        }
    }
}
