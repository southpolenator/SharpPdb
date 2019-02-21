using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents Portable PDB file reader of embedded PDB into assembly.
    /// </summary>
    public class EmbeddedPdbFile : IPdbFile
    {
        /// <summary>
        /// File loaded into memory for faster parsing.
        /// </summary>
        private MemoryLoadedFile file;

        /// <summary>
        /// Embedded PDB reader provider.
        /// </summary>
        private MetadataReaderProvider embeddedPdbReaderProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="file">File loaded into memory for faster parsing.</param>
        public unsafe EmbeddedPdbFile(MemoryLoadedFile file)
        {
            PEReader peReader = new PEReader(file.BasePointer, (int)file.Length);
            var debugEntries = peReader.ReadDebugDirectory();
            var embeddedPdbEntry = debugEntries.FirstOrDefault(e => e.Type == DebugDirectoryEntryType.EmbeddedPortablePdb);

            if (embeddedPdbEntry.DataSize == 0)
                throw new Exception("PDB wasn't embedded");
            embeddedPdbReaderProvider = peReader.ReadEmbeddedPortablePdbDebugDirectoryData(embeddedPdbEntry);
            PdbFile = new PdbFile(embeddedPdbReaderProvider.GetMetadataReader());
            this.file = file;
        }

        /// <summary>
        /// Gets the Portable PDB file reader.
        /// </summary>
        public PdbFile PdbFile { get; private set; }

        /// <summary>
        /// Gets the PDB file identifier.
        /// </summary>
        public Guid Guid => PdbFile.Guid;

        /// <summary>
        /// Gets the PDB stamp.
        /// </summary>
        public uint Stamp => PdbFile.Stamp;

        /// <summary>
        /// Gets the PDB file age.
        /// </summary>
        public int Age => PdbFile.Age;

        /// <summary>
        /// Gets the list of functions described in this PDB file.
        /// </summary>
        public IReadOnlyList<IPdbFunction> Functions => PdbFile.Functions;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            embeddedPdbReaderProvider?.Dispose();
            file?.Dispose();
        }

        /// <summary>
        /// Find function by the specified token.
        /// </summary>
        /// <param name="token">Method definition token.</param>
        /// <returns><see cref="IPdbFunction"/> object if found, <c>null</c> otherwise.</returns>
        public IPdbFunction GetFunctionFromToken(int token)
        {
            return PdbFile.GetFunctionFromToken(token);
        }
    }
}
