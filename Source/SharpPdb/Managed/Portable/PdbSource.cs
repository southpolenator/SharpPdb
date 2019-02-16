using SharpUtilities;
using System;
using System.Reflection.Metadata;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents source file referenced in Portable PDB file.
    /// </summary>
    public class PdbSource : IPdbSource
    {
        /// <summary>
        /// Cache for <see cref="Document"/> property.
        /// </summary>
        private SimpleCacheStruct<Document> documentCache;

        /// <summary>
        /// Cache for <see cref="Hash"/> property.
        /// </summary>
        private SimpleCacheStruct<byte[]> hashCache;

        /// <summary>
        /// Cache for <see cref="Name"/> property.
        /// </summary>
        private SimpleCacheStruct<string> nameCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSource"/> class.
        /// </summary>
        /// <param name="pdbFile">Portable PDB file reader.</param>
        /// <param name="handle">Our metadata reader handle.</param>
        internal PdbSource(PdbFile pdbFile, DocumentHandle handle)
        {
            PdbFile = pdbFile;
            documentCache = SimpleCache.CreateStruct(() => PdbFile.Reader.GetDocument(handle));
            hashCache = SimpleCache.CreateStruct(() => PdbFile.Reader.GetBlobBytes(Document.Hash));
            nameCache = SimpleCache.CreateStruct(() => PdbFile.Reader.GetString(Document.Name));
        }

        /// <summary>
        /// Gets the Portable PDB file reader.
        /// </summary>
        public PdbFile PdbFile { get; private set; }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public Document Document => documentCache.Value;

        /// <summary>
        /// Gets the hash value bytes.
        /// </summary>
        public byte[] Hash => hashCache.Value;

        /// <summary>
        /// Gets the hash algorithm id.
        /// </summary>
        public Guid HashAlgorithm => PdbFile.Reader.GetGuid(Document.HashAlgorithm);

        /// <summary>
        /// Gets the language id.
        /// </summary>
        public Guid Language => PdbFile.Reader.GetGuid(Document.Language);

        /// <summary>
        /// Gets the source file name.
        /// </summary>
        public string Name => nameCache.Value;
    }
}
