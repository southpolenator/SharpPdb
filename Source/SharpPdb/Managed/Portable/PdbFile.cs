using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents Portable PDB file reader.
    /// </summary>
    public class PdbFile : IPdbFile
    {
        /// <summary>
        /// File loaded into memory for faster parsing.
        /// </summary>
        private MemoryLoadedFile file;

        /// <summary>
        /// Cache for <see cref="Id"/> property.
        /// </summary>
        private SimpleCacheStruct<BlobContentId> idCache;

        /// <summary>
        /// Cache for <see cref="Functions"/> property.
        /// </summary>
        private SimpleCacheStruct<IPdbFunction[]> functionsCache;

        /// <summary>
        /// Cache for source files accessed by indexing operator.
        /// </summary>
        private DictionaryCache<DocumentHandle, IPdbSource> sourcesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="file">File loaded into memory for faster parsing.</param>
        public PdbFile(MemoryLoadedFile file)
        {
            unsafe
            {
                Reader = new MetadataReader(file.BasePointer, (int)file.Length);
            }
            this.file = file;
            idCache = SimpleCache.CreateStruct(() => new BlobContentId(Reader.DebugMetadataHeader.Id));
            functionsCache = SimpleCache.CreateStruct(() =>
            {
                IPdbFunction[] functions = new IPdbFunction[Reader.MethodDebugInformation.Count];
                int i = 0;

                foreach (var f in Reader.MethodDebugInformation)
                    functions[i++] = new PdbFunction(this, f);
                return functions;
            });
            sourcesCache = new DictionaryCache<DocumentHandle, IPdbSource>(GetSource);
        }

        /// <summary>
        /// Gets the portable PDB metadata reader.
        /// </summary>
        public MetadataReader Reader { get; private set; }

        /// <summary>
        /// Gets the PDB file blob content id.
        /// </summary>
        public BlobContentId Id => idCache.Value;

        /// <summary>
        /// Gets the PDB file identifier.
        /// </summary>
        public Guid Guid => Id.Guid;

        /// <summary>
        /// Gets the PDB stamp.
        /// </summary>
        public uint Stamp => Id.Stamp;

        /// <summary>
        /// Gets the list of functions described in this PDB file.
        /// </summary>
        public IReadOnlyList<IPdbFunction> Functions => functionsCache.Value;

        internal IPdbSource this[DocumentHandle sourceHandle] => sourcesCache[sourceHandle];

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            file?.Dispose();
        }

        /// <summary>
        /// Creates source file for the cache.
        /// </summary>
        /// <param name="sourceHandle">Handle of the source file.</param>
        private IPdbSource GetSource(DocumentHandle sourceHandle)
        {
            return new PdbSource(this, sourceHandle);
        }
    }
}
