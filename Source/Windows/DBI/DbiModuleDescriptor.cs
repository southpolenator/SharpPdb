using SharpPdb.Windows.Utility;
using SharpUtilities;

namespace SharpPdb.Windows.DBI
{
    /// <summary>
    /// DBI stream module description.
    /// </summary>
    public class DbiModuleDescriptor
    {
        /// <summary>
        /// Cache of files compiled in this module.
        /// </summary>
        private SimpleCacheStruct<string[]> filesCache;

        /// <summary>
        /// Cache of module stream.
        /// </summary>
        private SimpleCacheStruct<PdbStream> moduleStreamCache;

        /// <summary>
        /// Cache of local symbol debug info stream.
        /// </summary>
        private SimpleCacheStruct<SymbolStream> localSymbolStreamCache;

        /// <summary>
        /// Cache of debug subsection stream.
        /// </summary>
        private SimpleCacheStruct<DebugSubsectionStream> debugSubsectionStreamCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbiModuleDescriptor"/> class.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="moduleList">Owning module list.</param>
        public DbiModuleDescriptor(IBinaryReader reader, DbiModuleList moduleList)
        {
            ModuleList = moduleList;
            filesCache = SimpleCache.CreateStruct(() =>
            {
                string[] files = new string[NumberOfFiles];

                for (int i = 0; i < files.Length; i++)
                    files[i] = ModuleList.GetFileName(i + StartingFileIndex);
                return files;
            });
            Header = ModuleInfoHeader.Read(reader);
            ModuleName = reader.ReadCString();
            ObjectFileName = reader.ReadCString();

            // Descriptors should be aligned at 4 bytes
            if (reader.Position % 4 != 0)
                reader.Position += 4 - reader.Position % 4;

            // Higher level API initialization
            moduleStreamCache = SimpleCache.CreateStruct(() => moduleList.DbiStream.Stream.File.GetStream(ModuleStreamIndex));
            localSymbolStreamCache = SimpleCache.CreateStruct(() =>
            {
                IBinaryReader sreader = ModuleStream?.Reader?.Duplicate();

                if (sreader == null)
                    return null;
                sreader.Position = 0;
                int signature = sreader.ReadInt();

                if (signature != 4)
                    throw new System.Exception("Invalid signature of module stream");
                return new SymbolStream(sreader, SymbolDebugInfoByteSize);
            });
            debugSubsectionStreamCache = SimpleCache.CreateStruct(() =>
            {
                IBinaryReader sreader = ModuleStream.Reader.Duplicate();
                sreader.Position = SymbolDebugInfoByteSize + C11LineInfoByteSize;

                return new DebugSubsectionStream(sreader.ReadSubstream(C13LineInfoByteSize));
            });
        }

        /// <summary>
        /// Gets owning module list.
        /// </summary>
        public DbiModuleList ModuleList { get; private set; }

        /// <summary>
        /// Gets first file index from owning module list files array that is compiled into this module.
        /// </summary>
        public int StartingFileIndex { get; internal set; }

        /// <summary>
        /// Gets the header of this module descriptor.
        /// </summary>
        public ModuleInfoHeader Header { get; private set; }

        /// <summary>
        /// The module name. This is usually either a full path to an object file (either directly passed to link.exe or from an archive)
        /// or a string of the form Import:&lt;dll name&gt;.
        /// </summary>
        public StringReference ModuleName;

        /// <summary>
        /// The object file name. In the case of an module that is linked directly passed to link.exe, this is the same as <see cref="ModuleName"/>.
        /// In the case of a module that comes from an archive, this is usually the full path to the archive.
        /// </summary>
        public StringReference ObjectFileName;

        /// <summary>
        /// Gets all files compiled into this module.
        /// </summary>
        public string[] Files => filesCache.Value;

        /// <summary>
        /// Gets the module stream.
        /// </summary>
        public PdbStream ModuleStream => moduleStreamCache.Value;

        /// <summary>
        /// Gets the local symbol debug info stream.
        /// </summary>
        public SymbolStream LocalSymbolStream => localSymbolStreamCache.Value;

        /// <summary>
        /// Gets the debug subsection stream.
        /// </summary>
        public DebugSubsectionStream DebugSubsectionStream => debugSubsectionStreamCache.Value;

        #region Header data
        /// <summary>
        /// Stream index of module debug info.
        /// </summary>
        public ushort ModuleStreamIndex => Header.ModuleStreamIndex;

        /// <summary>
        /// Size of local symbol debug info in above stream
        /// </summary>
        public uint SymbolDebugInfoByteSize => Header.SymbolDebugInfoByteSize;

        /// <summary>
        /// Size of C11 line number info in above stream
        /// </summary>
        public uint C11LineInfoByteSize => Header.C11LineInfoByteSize;

        /// <summary>
        /// Size of C13 line number info in above stream
        /// </summary>
        public uint C13LineInfoByteSize => Header.C13LineInfoByteSize;

        /// <summary>
        /// Number of files contributing to this module
        /// </summary>
        public uint NumberOfFiles => Header.NumberOfFiles;

        /// <summary>
        /// Name Index for source file name.
        /// </summary>
        public uint SourceFileNameIndex => Header.SourceFileNameIndex;

        /// <summary>
        /// Name Index for path to compiler PDB.
        /// </summary>
        public uint PdbFilePathNameIndex => Header.PdbFilePathNameIndex;
        #endregion
    }
}
