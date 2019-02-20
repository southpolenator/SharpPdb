using SharpUtilities;
using System;

namespace SharpPdb.Windows.PIS
{
    /// <summary>
    /// PDB info stream header structure.
    /// </summary>
    public struct InfoStreamHeader
    {
        /// <summary>
        /// Expected size of the header in bytes.
        /// </summary>
        public const int Size = 28;

        /// <summary>
        /// Gets the version.
        /// </summary>
        public InfoStreamVersion Version { get; private set; }

        /// <summary>
        /// Gets the signature.
        /// </summary>
        public uint Signature { get; private set; }

        /// <summary>
        /// Gets the age.
        /// </summary>
        public uint Age { get; private set; }

        /// <summary>
        /// Gets the GUID.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Reads <see cref="InfoStreamHeader"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static InfoStreamHeader Read(IBinaryReader reader)
        {
            return new InfoStreamHeader
            {
                Version = (InfoStreamVersion)reader.ReadUint(),
                Signature = reader.ReadUint(),
                Age = reader.ReadUint(),
                Guid = reader.ReadGuid(),
            };
        }
    }
}
