using SharpPdb.Windows;
using SharpPdb.Windows.DebugSubsections;
using SharpPdb.Windows.SymbolRecords;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpPdb.Managed.Windows
{
    /// <summary>
    /// Represents Windows PDB file reader.
    /// </summary>
    public class PdbFile : IPdbFile
    {
        /// <summary>
        /// File loaded into memory for faster parsing.
        /// </summary>
        private MemoryLoadedFile file;

        /// <summary>
        /// Cache for <see cref="Functions"/> property.
        /// </summary>
        private SimpleCacheStruct<List<IPdbFunction>> functionsCache;

        /// <summary>
        /// Cache for <see cref="NamesStream"/> property.
        /// </summary>
        private SimpleCacheStruct<PdbStringTable> namesStreamCache;

        /// <summary>
        /// Cache for source files accessed by indexing operator.
        /// </summary>
        private DictionaryCache<FileChecksumSubsection, PdbSource> sourcesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="file">File loaded into memory for faster parsing.</param>
        public PdbFile(MemoryLoadedFile file)
        {
            Reader = new SharpPdb.Windows.PdbFile(file);
            functionsCache = SimpleCache.CreateStruct(() =>
            {
                List<IPdbFunction> functions = new List<IPdbFunction>();

                foreach (var dbiModule in Reader.DbiStream.Modules)
                {
                    var symbolStream = dbiModule.LocalSymbolStream;

                    foreach (var kind in ManagedProcedureSymbol.Kinds)
                        foreach (ManagedProcedureSymbol procedure in symbolStream[kind])
                            functions.Add(new PdbFunction(this, procedure, dbiModule));
                }
                return functions;
            });
            namesStreamCache = SimpleCache.CreateStruct(() =>
            {
                return new PdbStringTable(Reader.Streams[Reader.InfoStream.NamedStreamMap.Streams["/names"]].Reader);
            });
            sourcesCache = new DictionaryCache<FileChecksumSubsection, PdbSource>(checksum => new PdbSource(this, checksum));
            this.file = file;
        }

        /// <summary>
        /// Gets the Windows PDB reader.
        /// </summary>
        internal SharpPdb.Windows.PdbFile Reader { get; private set; }

        /// <summary>
        /// Gets the <c>/names</c> stream.
        /// </summary>
        public PdbStringTable NamesStream => namesStreamCache.Value;

        /// <summary>
        /// Gets the PDB file info stream header.
        /// </summary>
        public SharpPdb.Windows.PIS.InfoStreamHeader PdbId => Reader.InfoStream.Header;

        /// <summary>
        /// Gets the PDB file identifier.
        /// </summary>
        public Guid Guid => PdbId.Guid;

        /// <summary>
        /// Gets the PDB stamp.
        /// </summary>
        public uint Stamp => PdbId.Signature;

        /// <summary>
        /// Gets the list of functions described in this PDB file.
        /// </summary>
        public IReadOnlyList<IPdbFunction> Functions => functionsCache.Value;

        internal PdbSource this[FileChecksumSubsection checksum] => sourcesCache[checksum];

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            file?.Dispose();
        }
    }
}
