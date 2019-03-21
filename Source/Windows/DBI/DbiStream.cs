using SharpPdb.Windows.Utility;
using SharpUtilities;
using System;

namespace SharpPdb.Windows.DBI
{
    /// <summary>
    /// Represents debug info (DBI) stream from PDB file.
    /// </summary>
    public class DbiStream
    {
        /// <summary>
        /// Value for which stream index is invalid (stream doesn't exist).
        /// </summary>
        public const ushort InvalidStreamIndex = 0xFFFF;

        /// <summary>
        /// Cache for getting DBI module list.
        /// </summary>
        private SimpleCacheStruct<DbiModuleList> modulesCache;

        /// <summary>
        /// Cache for section contributions
        /// </summary>
        private SimpleCacheStruct<SectionContributionEntry[]> sectionContributionsCache;

        /// <summary>
        /// Cache for section contributions of newer version
        /// </summary>
        private SimpleCacheStruct<SectionContributionEntry2[]> sectionContributions2Cache;

        /// <summary>
        /// Cache for <see cref="SectionHeaders"/> property.
        /// </summary>
        private SimpleCacheStruct<CoffSectionHeader[]> sectionHeadersCache;

        /// <summary>
        /// Cache for <see cref="OriginalSectionHeaders"/> property.
        /// </summary>
        private SimpleCacheStruct<CoffSectionHeader[]> originalSectionHeadersCache;

        /// <summary>
        /// Cache for section map.
        /// </summary>
        private SimpleCacheStruct<SectionMapEntry[]> sectionMapCache;

        /// <summary>
        /// Cache for FPO stream.
        /// </summary>
        private SimpleCacheStruct<IBinaryReader> fpoStreamCache;

        /// <summary>
        /// Cache for FPO records.
        /// </summary>
        private SimpleCacheStruct<FpoData[]> fpoRecordsCache;

        /// <summary>
        /// Cache for <see cref="OmapToSourceEntries"/> property.
        /// </summary>
        private SimpleCacheStruct<OmapEntry[]> omapToSourceEntriesCache;

        /// <summary>
        /// Cache for <see cref="OmapFromSourceEntries"/> property.
        /// </summary>
        private SimpleCacheStruct<OmapEntry[]> omapFromSourceEntriesCache;

        /// <summary>
        /// Cache for EC names string table.
        /// </summary>
        private SimpleCacheStruct<PdbStringTable> ecNamesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbiStream"/> class.
        /// </summary>
        /// <param name="stream">PDB stream that contains DBI stream.</param>
        public DbiStream(PdbStream stream)
        {
            Stream = stream;
            stream.Reader.Position = 0;
            if (Stream.Length < DbiStreamHeader.Size)
                throw new Exception("DBI Stream does not contain a header.");
            Header = DbiStreamHeader.Read(stream.Reader);
            if (Header.VersionSignature != -1)
                throw new Exception("Invalid DBI version signature.");

            // Require at least version 7, which should be present in all PDBs
            // produced in the last decade and allows us to avoid having to
            // special case all kinds of complicated arcane formats.
            if (Header.Version < DbiStreamVersion.V70)
                throw new Exception("Unsupported DBI version.");

            int expectedSize = DbiStreamHeader.Size + Header.ModuleInfoSubstreamSize + Header.SectionContributionSubstreamSize +
                Header.SectionMapSize + Header.FileInfoSize + Header.TypeServerSize +
                Header.OptionalDebugHeaderSize + Header.ECSubstreamSize;
            if (Stream.Length != expectedSize)
                throw new Exception("DBI Length does not equal sum of substreams.");

            // Only certain substreams are guaranteed to be aligned. Validate
            // them here.
            if (Header.ModuleInfoSubstreamSize % 4 != 0)
                throw new Exception("DBI MODI substream not aligned.");
            if (Header.SectionContributionSubstreamSize % 4 != 0)
                throw new Exception("DBI section contribution substream not aligned.");
            if (Header.SectionMapSize % 4 != 0)
                throw new Exception("DBI section map substream not aligned.");
            if (Header.FileInfoSize % 4 != 0)
                throw new Exception("DBI file info substream not aligned.");
            if (Header.TypeServerSize % 4 != 0)
                throw new Exception("DBI type server substream not aligned.");

            // Get substreams
            ModuleInfoSubstream = stream.Reader.ReadSubstream(Header.ModuleInfoSubstreamSize);
            SectionContributionSubstream = stream.Reader.ReadSubstream(Header.SectionContributionSubstreamSize);
            SectionMapSubstream = stream.Reader.ReadSubstream(Header.SectionMapSize);
            FileInfoSubstream = stream.Reader.ReadSubstream(Header.FileInfoSize);
            TypeServerMapSubstream = stream.Reader.ReadSubstream(Header.TypeServerSize);
            ECSubstream = stream.Reader.ReadSubstream(Header.ECSubstreamSize);
            DebugStreamIndexes = stream.Reader.ReadUshortArray(Header.OptionalDebugHeaderSize / 2);
            if (stream.Reader.BytesRemaining > 0)
                throw new Exception("Found unexpected bytes in DBI Stream.");

            // Create caches for reading substreams
            modulesCache = SimpleCache.CreateStruct(() =>
            {
                ModuleInfoSubstream.Position = 0;
                FileInfoSubstream.Position = 0;
                return new DbiModuleList(ModuleInfoSubstream, FileInfoSubstream, this);
            });
            if (SectionContributionSubstream.Length > 0)
            {
                DbiSectionContributionVersion version = (DbiSectionContributionVersion)SectionContributionSubstream.ReadUint();

                if (version != DbiSectionContributionVersion.V60 && version != DbiSectionContributionVersion.V2)
                    throw new Exception("Unsupported DBI Section Contribution version");
            }
            sectionContributionsCache = SimpleCache.CreateStruct(() =>
            {
                SectionContributionEntry[] result = null;

                if (SectionContributionSubstream.Length > 0)
                {
                    SectionContributionSubstream.Position = 0;
                    DbiSectionContributionVersion version = (DbiSectionContributionVersion)SectionContributionSubstream.ReadUint();

                    if (version == DbiSectionContributionVersion.V60)
                    {
                        // Read array from the stream
                        result = new SectionContributionEntry[SectionContributionSubstream.BytesRemaining / SectionContributionEntry.Size];
                        for (int i = 0; i < result.Length; i++)
                            result[i] = SectionContributionEntry.Read(SectionContributionSubstream);
                    }
                    else
                    {
                        // Copy values from newer section contributions since it is expanded version of us.
                        SectionContributionEntry2[] result2 = SectionContributions2;

                        if (result2 != null)
                        {
                            result = new SectionContributionEntry[result2.Length];
                            for (int i = 0; i < result.Length; i++)
                                result[i] = result2[i].Base;
                        }
                    }
                }
                return result;
            });
            sectionContributions2Cache = SimpleCache.CreateStruct(() =>
            {
                SectionContributionEntry2[] result = null;

                if (SectionContributionSubstream.Length > 0)
                {
                    SectionContributionSubstream.Position = 0;
                    DbiSectionContributionVersion version = (DbiSectionContributionVersion)SectionContributionSubstream.ReadUint();

                    if (version == DbiSectionContributionVersion.V2)
                    {
                        // Read array from the stream
                        result = new SectionContributionEntry2[SectionContributionSubstream.BytesRemaining / SectionContributionEntry2.Size];
                        for (int i = 0; i < result.Length; i++)
                            result[i] = SectionContributionEntry2.Read(SectionContributionSubstream);
                    }
                }
                return result;
            });

            CoffSectionHeader[] ReadSectionHeadersStream(IBinaryReader sectionHeaderStream)
            {
                if (sectionHeaderStream != null)
                {
                    sectionHeaderStream.Position = 0;
                    if (sectionHeaderStream.Length % CoffSectionHeader.Size != 0)
                        throw new Exception("Corrupted section header stream.");

                    int numSections = (int)(sectionHeaderStream.Length / CoffSectionHeader.Size);
                    CoffSectionHeader[] sectionHeaders = new CoffSectionHeader[numSections];

                    for (int i = 0; i < numSections; i++)
                        sectionHeaders[i] = CoffSectionHeader.Read(sectionHeaderStream);
                    return sectionHeaders;
                }
                return null;
            };

            sectionHeadersCache = SimpleCache.CreateStruct(() => ReadSectionHeadersStream(GetKnownDebugStream(KnownDebugStreamIndex.SectionHdr)?.Reader));
            originalSectionHeadersCache = SimpleCache.CreateStruct(() => ReadSectionHeadersStream(GetKnownDebugStream(KnownDebugStreamIndex.SectionHdrOrig)?.Reader));
            sectionMapCache = SimpleCache.CreateStruct(() =>
            {
                if (SectionMapSubstream.Length > 0)
                {
                    SectionMapSubstream.Position = 0;
                    ushort secCount = SectionMapSubstream.ReadUshort();
                    ushort secCountLog = SectionMapSubstream.ReadUshort();

                    SectionMapEntry[] sectionMap = new SectionMapEntry[secCount];
                    for (int i = 0; i < sectionMap.Length; i++)
                        sectionMap[i] = SectionMapEntry.Read(SectionMapSubstream);
                    return sectionMap;
                }
                return null;
            });
            fpoStreamCache = SimpleCache.CreateStruct(() => GetKnownDebugStream(KnownDebugStreamIndex.FPO)?.Reader);
            fpoRecordsCache = SimpleCache.CreateStruct(() =>
            {
                if (FpoStream != null)
                {
                    FpoStream.Position = 0;
                    if (FpoStream.Length % FpoData.Size != 0)
                        throw new Exception("Corrupted New FPO stream.");

                    int numRecords = (int)(FpoStream.Length / FpoData.Size);
                    FpoData[] fpoRecords = new FpoData[numRecords];

                    for (int i = 0; i < numRecords; i++)
                        fpoRecords[i] = FpoData.Read(FpoStream);
                    return fpoRecords;
                }
                return null;
            });
            OmapEntry[] LoadOmapStream(IBinaryReader omapReader)
            {
                if (omapReader != null)
                {
                    omapReader.Position = 0;
                    if (omapReader.Length % OmapEntry.Size != 0)
                        throw new Exception("Corrupted Omap stream.");

                    int count = (int)(omapReader.Length / OmapEntry.Size);
                    OmapEntry[] entries = new OmapEntry[count];

                    for (int i = 0; i < count; i++)
                        entries[i] = OmapEntry.Read(omapReader);
                    return entries;
                }
                return null;
            };

            omapToSourceEntriesCache = SimpleCache.CreateStruct(() => LoadOmapStream(GetKnownDebugStream(KnownDebugStreamIndex.OmapToSrc)?.Reader));
            omapFromSourceEntriesCache = SimpleCache.CreateStruct(() => LoadOmapStream(GetKnownDebugStream(KnownDebugStreamIndex.OmapFromSrc)?.Reader));
            ecNamesCache = SimpleCache.CreateStruct(() => ECSubstream.Length > 0 ? new PdbStringTable(ECSubstream) : null);
        }

        /// <summary>
        /// Gets the associated PDB stream.
        /// </summary>
        public PdbStream Stream { get; private set; }

        /// <summary>
        /// Gets DBI stream header.
        /// </summary>
        public DbiStreamHeader Header { get; private set; }

        /// <summary>
        /// Gets the module info substream.
        /// </summary>
        public IBinaryReader ModuleInfoSubstream { get; private set; }

        /// <summary>
        /// Gets the section contribution substream.
        /// </summary>
        public IBinaryReader SectionContributionSubstream { get; private set; }

        /// <summary>
        /// Gets the section map substream.
        /// </summary>
        public IBinaryReader SectionMapSubstream { get; private set; }

        /// <summary>
        /// Gets the file info substream.
        /// </summary>
        public IBinaryReader FileInfoSubstream { get; private set; }

        /// <summary>
        /// Gets the type server map substream.
        /// </summary>
        public IBinaryReader TypeServerMapSubstream { get; private set; }

        /// <summary>
        /// Gets the EC substream (what is EC?)
        /// </summary>
        public IBinaryReader ECSubstream { get; private set; }

        /// <summary>
        /// Gets indexes of known debug streams. Use <see cref="KnownDebugStreamIndex"/> to access this array.
        /// </summary>
        public ushort[] DebugStreamIndexes { get; private set; }

        /// <summary>
        /// Gets the global symbol stream index.
        /// </summary>
        public ushort GlobalSymbolStreamIndex => Header.GlobalSymbolStreamIndex;

        /// <summary>
        /// Gets the symbol records stream index.
        /// </summary>
        public ushort SymbolRecordStreamIndex => Header.SymbolRecordStreamIndex;

        /// <summary>
        /// Gets the DBI module list.
        /// </summary>
        public DbiModuleList Modules => modulesCache.Value;

        /// <summary>
        /// Gets the section contributions (V1). If V2 is available, <see cref="SectionContributions2"/> will not be <c>null</c> and this will contain same data too.
        /// </summary>
        public SectionContributionEntry[] SectionContributions => sectionContributionsCache.Value;


        /// <summary>
        /// Gets the section contributions (V2). It will be <c>null</c> if <see cref="SectionContributionSubstream"/> has V1 entries.
        /// </summary>
        public SectionContributionEntry2[] SectionContributions2 => sectionContributions2Cache.Value;

        /// <summary>
        /// Gets the COFF section headers.
        /// </summary>
        public CoffSectionHeader[] SectionHeaders => sectionHeadersCache.Value;

        /// <summary>
        /// Gets the original COFF section headers.
        /// </summary>
        public CoffSectionHeader[] OriginalSectionHeaders => originalSectionHeadersCache.Value;

        /// <summary>
        /// Gets the section map.
        /// </summary>
        public SectionMapEntry[] SectionMap => sectionMapCache.Value;

        /// <summary>
        /// Gets the FPO stream.
        /// </summary>
        public IBinaryReader FpoStream => fpoStreamCache.Value;

        /// <summary>
        /// Gets the FPO records.
        /// </summary>
        public FpoData[] FpoRecords => fpoRecordsCache.Value;

        /// <summary>
        /// Parsed entries of the <see cref="KnownDebugStreamIndex.OmapToSrc"/> stream.
        /// </summary>
        public OmapEntry[] OmapToSourceEntries => omapToSourceEntriesCache.Value;

        /// <summary>
        /// Parsed entries of the <see cref="KnownDebugStreamIndex.OmapFromSrc"/> stream.
        /// </summary>
        public OmapEntry[] OmapFromSourceEntries => omapFromSourceEntriesCache.Value;

        /// <summary>
        /// Gets the EC names string table.
        /// </summary>
        public PdbStringTable ECNames => ecNamesCache.Value;

        /// <summary>
        /// Safely gets known debug PDB stream. It will return <c>null</c> if it is out of bounds.
        /// </summary>
        /// <param name="knownDebugStreamIndex">Known debug stream index.</param>
        public PdbStream GetKnownDebugStream(KnownDebugStreamIndex knownDebugStreamIndex)
        {
            int knownIndex = (int)knownDebugStreamIndex;

            if (knownIndex < 0 || knownIndex >= DebugStreamIndexes.Length)
                return null;

            int streamIndex = DebugStreamIndexes[knownIndex];

            return Stream.File.GetStream(streamIndex);
        }
    }
}
