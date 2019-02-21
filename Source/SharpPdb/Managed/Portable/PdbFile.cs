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
        /// Dictionary cache of functions by handle.
        /// </summary>
        private DictionaryCache<MethodDebugInformationHandle, PdbFunction> functionsByHandle;

        /// <summary>
        /// Cache for source files accessed by indexing operator.
        /// </summary>
        private DictionaryCache<DocumentHandle, IPdbSource> sourcesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="file">File loaded into memory for faster parsing.</param>
        public unsafe PdbFile(MemoryLoadedFile file)
            : this(new MetadataReader(file.BasePointer, (int)file.Length))
        {
            this.file = file;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="reader">Portable PDB metadata reader.</param>
        internal PdbFile(MetadataReader reader)
        {
            Reader = reader;
            idCache = SimpleCache.CreateStruct(() => new BlobContentId(Reader.DebugMetadataHeader.Id));
            functionsCache = SimpleCache.CreateStruct(() =>
            {
                IPdbFunction[] functions = new IPdbFunction[Reader.MethodDebugInformation.Count];
                int i = 0;

                foreach (var f in Reader.MethodDebugInformation)
                    functions[i++] = functionsByHandle[f];
                return functions;
            });
            sourcesCache = new DictionaryCache<DocumentHandle, IPdbSource>(GetSource);
            functionsByHandle = new DictionaryCache<MethodDebugInformationHandle, PdbFunction>(f => new PdbFunction(this, f));
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
        /// Gets the PDB file age.
        /// </summary>
        public int Age => 1;

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
        /// Find function by the specified token.
        /// </summary>
        /// <param name="token">Method definition token.</param>
        /// <returns><see cref="IPdbFunction"/> object if found, <c>null</c> otherwise.</returns>
        public IPdbFunction GetFunctionFromToken(int token)
        {
            EntityHandle handle = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(token);
            int rowNumber = System.Reflection.Metadata.Ecma335.MetadataTokens.GetRowNumber(handle);
            MethodDebugInformationHandle methodHandle = System.Reflection.Metadata.Ecma335.MetadataTokens.MethodDebugInformationHandle(rowNumber);

            try
            {
                PdbFunction function = functionsByHandle[methodHandle];

                if (function != null)
                {
                    // Verify that returned function is valid.
                    var document = function.MethodDebugInformation.Document;
                }
                return function;
            }
            catch
            {
                functionsByHandle[methodHandle] = null;
                return null;
            }
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
