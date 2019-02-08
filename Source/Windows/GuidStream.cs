using SharpPdb.Windows.Utility;
using System;

namespace SharpPdb.Windows
{
    /// <summary>
    /// Represents guid stream.
    /// </summary>
    public class GuidStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuidStream"/> class.
        /// </summary>
        /// <param name="stream">PDB symbol stream.</param>
        public GuidStream(PdbStream stream)
        {
            Stream = stream;
            Language = stream.Reader.ReadGuid();
            Vendor = stream.Reader.ReadGuid();
            DocumentType = stream.Reader.ReadGuid();
            Algorithm = stream.Reader.ReadGuid();
            uint checksumSize = stream.Reader.ReadUint();
            uint sourceSize = stream.Reader.ReadUint();
            ChecksumReader = stream.Reader.ReadSubstream(checksumSize);
            SourceReader = stream.Reader.ReadSubstream(sourceSize);
        }

        /// <summary>
        /// Gets the PDB stream.
        /// </summary>
        public PdbStream Stream { get; private set; }

        /// <summary>
        /// Gets the language guid.
        /// </summary>
        public Guid Language { get; private set; }

        /// <summary>
        /// Gets the vendor guid.
        /// </summary>
        public Guid Vendor { get; private set; }

        /// <summary>
        /// Gets the document type guid.
        /// </summary>
        public Guid DocumentType { get; private set; }

        /// <summary>
        /// Gets the algorithm guid.
        /// </summary>
        public Guid Algorithm { get; private set; }

        /// <summary>
        /// Gets the checksum data binary reader.
        /// </summary>
        public IBinaryReader ChecksumReader { get; private set; }

        /// <summary>
        /// Gets the source file text binary reader.
        /// </summary>
        public IBinaryReader SourceReader { get; private set; }
    }
}
