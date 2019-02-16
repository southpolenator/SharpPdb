using SharpPdb.Windows;
using SharpPdb.Windows.DebugSubsections;
using SharpPdb.Windows.Utility;
using SharpUtilities;
using System;

namespace SharpPdb.Managed.Windows
{
    /// <summary>
    /// Represents source file referenced in Windows PDB file.
    /// </summary>
    public class PdbSource : IPdbSource
    {
        /// <summary>
        /// Cache for <see cref="Hash"/> property.
        /// </summary>
        private SimpleCacheStruct<byte[]> hashCache;

        /// <summary>
        /// Cache for <see cref="Name"/> property.
        /// </summary>
        private SimpleCacheStruct<string> nameCache;

        /// <summary>
        /// Cache for <see cref="GuidStream"/> property.
        /// </summary>
        private SimpleCacheStruct<GuidStream> guidStreamCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSource"/> class.
        /// </summary>
        /// <param name="pdbFile">Portable PDB file reader.</param>
        /// <param name="checksum">File checksum subsection.</param>
        internal PdbSource(PdbFile pdbFile, FileChecksumSubsection checksum)
        {
            PdbFile = pdbFile;
            Checksum = checksum;
            hashCache = SimpleCache.CreateStruct(() => GuidStream?.ChecksumReader.ReadAllBytes() ?? Checksum.HashReader.ReadAllBytes());
            nameCache = SimpleCache.CreateStruct(() => PdbFile.NamesStream.Dictionary[checksum.NameIndex]);
            guidStreamCache = SimpleCache.CreateStruct(() =>
            {
                int guidStreamIndex;

                if (PdbFile.Reader.InfoStream.NamedStreamMap.Streams.TryGetValue("/src/files/" + Name, out guidStreamIndex)
                    || PdbFile.Reader.InfoStream.NamedStreamMap.StreamsUppercase.TryGetValue("/SRC/FILES/" + Name.ToUpperInvariant(), out guidStreamIndex))
                {
                    return new GuidStream(PdbFile.Reader.Streams[guidStreamIndex]);
                }
                return null;
            });
        }

        /// <summary>
        /// Gets the Windows PDB file reader.
        /// </summary>
        public PdbFile PdbFile { get; private set; }

        /// <summary>
        /// Gets the file checksum subsection.
        /// </summary>
        public FileChecksumSubsection Checksum { get; private set; }

        /// <summary>
        /// Gets the guid stream associated with this source file.
        /// </summary>
        public GuidStream GuidStream => guidStreamCache.Value;

        /// <summary>
        /// Gets the hash value bytes.
        /// </summary>
        public byte[] Hash => hashCache.Value;

        /// <summary>
        /// Gets the hash algorithm id.
        /// </summary>
        public Guid HashAlgorithm => GuidStream.Algorithm;

        /// <summary>
        /// Gets the language id.
        /// </summary>
        public Guid Language => GuidStream.Language;

        /// <summary>
        /// Gets the source file name.
        /// </summary>
        public string Name => nameCache.Value;
    }
}
