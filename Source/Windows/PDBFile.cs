using SharpPdb.Windows.DBI;
using SharpPdb.Windows.GSI;
using SharpPdb.Windows.MSF;
using SharpPdb.Windows.PIS;
using SharpPdb.Windows.TPI;
using SharpPdb.Windows.Utility;
using SharpUtilities;
using System;
using System.Collections.Generic;

// TODO: Copy rest of the known symbols ( llvm/include/llvm/DebugInfo/CodeView/SymbolRecord.h, llvm/lib/DebugInfo/CodeView/SymbolRecordMapping.cpp )
// TODO: Copy rest of the known records ( llvm/include/llvm/DebugInfo/CodeView/TypeRecord.h, llvm/lib/DebugInfo/CodeView/TypeRecordMapping.cpp )

namespace SharpPdb.Windows
{
    /// <summary>
    /// Represents PDB file parser.
    /// </summary>
    public class PdbFile : IDisposable
    {
        /// <summary>
        /// PDB streams that exist in this PDB file.
        /// </summary>
        private PdbStream[] streams;

        /// <summary>
        /// Cache for <see cref="DbiStream"/>.
        /// </summary>
        private SimpleCacheStruct<DbiStream> dbiStreamCache;

        /// <summary>
        /// Cache for <see cref="InfoStream"/>.
        /// </summary>
        private SimpleCacheStruct<InfoStream> infoStreamCache;

        /// <summary>
        /// Cache for <see cref="PdbSymbolStream"/>.
        /// </summary>
        private SimpleCacheStruct<SymbolStream> pdbSymbolStreamCache;

        /// <summary>
        /// Cache for <see cref="GlobalsStream"/>.
        /// </summary>
        private SimpleCacheStruct<GlobalsStream> globalsStreamCache;

        /// <summary>
        /// Cache for <see cref="TpiStream"/>.
        /// </summary>
        private SimpleCacheStruct<TpiStream> tpiStreamCache;

        /// <summary>
        /// Cache for <see cref="IpiStream"/>.
        /// </summary>
        private SimpleCacheStruct<TpiStream> ipiStreamCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="path">Path to PDB file.</param>
        public PdbFile(string path)
        {
            MemoryLoadedFile file = new MemoryLoadedFile(path);
            try
            {
                Initialize(file);
            }
            catch
            {
                file.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="file">File loaded into memory. Note that file will be closed when instance of this type is disposed.</param>
        public PdbFile(MemoryLoadedFile file)
        {
            Initialize(file);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFile"/> class.
        /// </summary>
        /// <param name="reader">Binary stream reader of the PDB file.</param>
        public PdbFile(IBinaryReader reader)
        {
            Initialize(reader);
        }

        private void Initialize(MemoryLoadedFile file)
        {
            File = file;
            Initialize(new MemoryLoadedFileReader(file));
        }

        private void Initialize(IBinaryReader reader)
        {
            Reader = reader;

            // Parse file headers

            // Initialize MSF super block
            SuperBlock = MSF.SuperBlock.Read(Reader);

            SuperBlock.Validate();
            if (File.Length % SuperBlock.BlockSize != 0)
                throw new Exception("File size is not a multiple of block size");

            // Initialize Free Page Map.
            // The Fpm exists either at block 1 or block 2 of the MSF. However, this
            // allows for a maximum of getBlockSize() * 8 blocks bits in the Fpm, and
            // thusly an equal number of total blocks in the file. For a block size
            // of 4KiB (very common), this would yield 32KiB total blocks in file, for a
            // maximum file size of 32KiB * 4KiB = 128MiB. Obviously this won't do, so
            // the Fpm is split across the file at `getBlockSize()` intervals.  As a
            // result, every block whose index is of the form |{1,2} + getBlockSize() * k|
            // for any non-negative integer k is an Fpm block. In theory, we only really
            // need to reserve blocks of the form |{1,2} + getBlockSize() * 8 * k|, but
            // current versions of the MSF format already expect the Fpm to be arranged
            // at getBlockSize() intervals, so we have to be compatible.
            // See the function fpmPn() for more information:
            // https://github.com/Microsoft/microsoft-pdb/blob/master/PDB/msf/msf.cpp#L489
            uint fpmIntervals = (SuperBlock.NumBlocks + 8 * SuperBlock.BlockSize - 1) / (8 * SuperBlock.BlockSize);
            uint[] fpmBlocks = new uint[fpmIntervals];
            uint currentFpmBlock = SuperBlock.FreeBlockMapBlock;

            for (int i = 0; i < fpmBlocks.Length; i++)
            {
                fpmBlocks[i] = currentFpmBlock;
                currentFpmBlock += SuperBlock.BlockSize;
            }

            IBinaryReader fpmStream = new MappedBlockBinaryReader(fpmBlocks, SuperBlock.BlockSize, (SuperBlock.NumBlocks + 7) / 8, Reader);
            FreePageMap = Reader.ReadByteArray((int)fpmStream.Length);

            // Read directory blocks
            Reader.Position = (long)SuperBlock.BlockMapOffset;
            uint[] directoryBlocks = Reader.ReadUintArray((int)SuperBlock.NumDirectoryBlocks);

            // Parse stream data
            uint NumStreams = 0;
            PdbStream directoryStream = new PdbStream(directoryBlocks, SuperBlock.NumDirectoryBytes, this);

            NumStreams = directoryStream.Reader.ReadUint();
            streams = new PdbStream[NumStreams];
            uint[] streamSizes = directoryStream.Reader.ReadUintArray(streams.Length);
            for (int i = 0; i < streams.Length; i++)
            {
                uint streamSize = streamSizes[i];
                uint NumExpectedStreamBlocks = streamSize == uint.MaxValue ? 0 : SuperBlock.BytesToBlocks(streamSize);
                uint[] blocks = directoryStream.Reader.ReadUintArray((int)NumExpectedStreamBlocks);

                foreach (uint block in blocks)
                {
                    ulong blockEndOffset = SuperBlock.BlocksToBytes(block + 1);

                    if (blockEndOffset > (ulong)File.Length)
                        throw new Exception("Stream block map is corrupt.");
                }

                streams[i] = new PdbStream(blocks, streamSize, this);
            }

            if (directoryStream.Reader.Position != SuperBlock.NumDirectoryBytes)
                throw new Exception("Not whole directory stream was read");

            dbiStreamCache = SimpleCache.CreateStruct(() => new DbiStream(streams[(uint)SpecialStream.StreamDBI]));
            infoStreamCache = SimpleCache.CreateStruct(() => new InfoStream(streams[(uint)SpecialStream.StreamPDB]));
            tpiStreamCache = SimpleCache.CreateStruct(() => new TpiStream(streams[(uint)SpecialStream.StreamTPI]));
            ipiStreamCache = SimpleCache.CreateStruct(() => new TpiStream(streams[(uint)SpecialStream.StreamIPI]));
            pdbSymbolStreamCache = SimpleCache.CreateStruct(() =>
            {
                PdbStream stream = GetStream(DbiStream.SymbolRecordStreamIndex);

                if (stream != null)
                    return new SymbolStream(stream);
                return null;
            });
            globalsStreamCache = SimpleCache.CreateStruct(() =>
            {
                PdbStream stream = GetStream(DbiStream.GlobalSymbolStreamIndex);

                if (stream != null)
                    return new GlobalsStream(stream);
                return null;
            });
        }

        /// <summary>
        /// Gets the super block.
        /// </summary>
        public SuperBlock SuperBlock { get; private set; }

        /// <summary>
        /// Gets bit array of free pages in this PDB file.
        /// </summary>
        public byte[] FreePageMap { get; private set; }

        /// <summary>
        /// Gets number of bits in <see cref="FreePageMap"/> array.
        /// </summary>
        public uint FreePageMapBitLength => SuperBlock.NumBlocks;

        /// <summary>
        /// Gets parsed DBI stream.
        /// </summary>
        public DbiStream DbiStream => dbiStreamCache.Value;

        /// <summary>
        /// Gets parsed PDB info stream.
        /// </summary>
        public InfoStream InfoStream => infoStreamCache.Value;

        /// <summary>
        /// Gets parsed symbol stream.
        /// </summary>
        public SymbolStream PdbSymbolStream => pdbSymbolStreamCache.Value;

        /// <summary>
        /// Gets parsed globals stream.
        /// </summary>
        public GlobalsStream GlobalsStream => globalsStreamCache.Value;

        /// <summary>
        /// Gets parsed TPI stream.
        /// </summary>
        public TpiStream TpiStream => tpiStreamCache.Value;

        /// <summary>
        /// Gets parsed IPI stream.
        /// </summary>
        public TpiStream IpiStream => ipiStreamCache.Value;

        /// <summary>
        /// Gets the list of PDB streams in this PDB file.
        /// </summary>
        public IReadOnlyList<PdbStream> Streams => streams;

        /// <summary>
        /// Gets the memory mapped file associated with this PDB file.
        /// </summary>
        internal MemoryLoadedFile File { get; private set; }

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        internal IBinaryReader Reader { get; private set; }

        /// <summary>
        /// Resolves relative virtual address for the specified segment and offset.
        /// Relative virtual address is offset from module load address.
        /// </summary>
        /// <param name="segment">Section where symbol is located.</param>
        /// <param name="offset">Offset within the section.</param>
        public ulong FindRelativeVirtualAddress(ushort segment, uint offset)
        {
            var dbi = DbiStream;
            var sections = dbi?.SectionHeaders;

            if (sections == null || segment == 0 || segment > sections.Length)
                return 0;
            return sections[segment - 1].VirtualAddress + offset;
        }

        /// <summary>
        /// Safely gets the PDB stream from <see cref="Streams"/>. It will return <c>null</c> if index is outside of the range.
        /// </summary>
        /// <param name="streamIndex">Index in the <see cref="Streams"/> list.</param>
        public PdbStream GetStream(int streamIndex)
        {
            if (streamIndex > 0 && streamIndex < Streams.Count)
                return Streams[streamIndex];
            return null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            File?.Dispose();
        }
    }
}
