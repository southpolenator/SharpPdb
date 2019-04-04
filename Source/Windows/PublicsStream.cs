using SharpPdb.Windows.GSI;
using SharpPdb.Windows.SymbolRecords;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpPdb.Windows
{
    /// <summary>
    /// Publics stream header structure.
    /// </summary>
    public struct PublicsStreamHeader
    {
        /// <summary>
        /// Size of <see cref="PublicsStreamHeader"/> structure in bytes.
        /// </summary>
        public const int Size = 28;

        /// <summary>
        /// Gets the globals stream size in bytes.
        /// </summary>
        public uint SymbolHashStreamSize { get; private set; }

        /// <summary>
        /// Gets the address map size in bytes.
        /// </summary>
        public uint AddressMapSize { get; private set; }

        /// <summary>
        /// Gets the number of thunks.
        /// </summary>
        public uint NumberOfThunks { get; private set; }

        /// <summary>
        /// Gets the size of the thunk.
        /// </summary>
        public uint SizeOfThunk { get; private set; }

        /// <summary>
        /// Gets the thunk table section index.
        /// </summary>
        public ushort ThunkTableSection { get; private set; }

        /// <summary>
        /// Gets the padding (unused).
        /// </summary>
        public ushort Padding { get; private set; }

        /// <summary>
        /// Gets the thunk table offset.
        /// </summary>
        public uint ThunkTableOffset { get; private set; }

        /// <summary>
        /// Gets the number of sections.
        /// </summary>
        public uint NumberOfSections { get; private set; }

        /// <summary>
        /// Reads <see cref="PublicsStreamHeader"/> from the stream.
        /// </summary>
        /// <param name="reader">Strem binary reader.</param>
        public static PublicsStreamHeader Read(IBinaryReader reader)
        {
            return new PublicsStreamHeader
            {
                SymbolHashStreamSize = reader.ReadUint(),
                AddressMapSize = reader.ReadUint(),
                NumberOfThunks = reader.ReadUint(),
                SizeOfThunk = reader.ReadUint(),
                ThunkTableSection = reader.ReadUshort(),
                Padding = reader.ReadUshort(),
                ThunkTableOffset = reader.ReadUint(),
                NumberOfSections = reader.ReadUint(),
            };
        }
    }

    /// <summary>
    /// Publics stream section offset structure.
    /// </summary>
    public struct PublicsStreamSectionOffset
    {
        /// <summary>
        /// Size of <see cref="PublicsStreamSectionOffset"/> structure in bytes.
        /// </summary>
        public const int Size = 8;

        /// <summary>
        /// Gets the offset inside the section.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the section index.
        /// </summary>
        public ushort Section { get; private set; }

        /// <summary>
        /// Gets the padding (unused).
        /// </summary>
        public ushort Padding { get; private set; }

        /// <summary>
        /// Reads <see cref="PublicsStreamSectionOffset"/> from the stream.
        /// </summary>
        /// <param name="reader">Strem binary reader.</param>
        public static PublicsStreamSectionOffset Read(IBinaryReader reader)
        {
            return new PublicsStreamSectionOffset
            {
                Offset = reader.ReadUint(),
                Section = reader.ReadUshort(),
                Padding = reader.ReadUshort(),
            };
        }
    }

    /// <summary>
    /// Represents PDB publics stream.
    /// </summary>
    public class PublicsStream
    {
        /// <summary>
        /// Cache for <see cref="PublicSymbols"/> property.
        /// </summary>
        private SimpleCacheStruct<Public32Symbol[]> publicSymbolsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicsStream"/> class.
        /// </summary>
        /// <param name="file">PDB file containing this stream.</param>
        /// <param name="reader">Binary stream reader.</param>
        public PublicsStream(PdbFile file, IBinaryReader reader)
        {
            if (reader.BytesRemaining < PublicsStreamHeader.Size)
                throw new Exception("Publics Stream does not contain a header.");

            // Read header
            Header = PublicsStreamHeader.Read(reader);

            // Read globals hash table
            if (reader.BytesRemaining < Header.SymbolHashStreamSize)
                throw new Exception("Publics Stream does not contain a header.");
            GlobalsStream = new GlobalsStream(file, reader.ReadSubstream(Header.SymbolHashStreamSize));

            // Read address map
            if (reader.BytesRemaining < Header.AddressMapSize)
                throw new Exception("Could not read an address map.");
            AddressMap = reader.ReadUintArray((int)(Header.AddressMapSize / 4)); // 4 = sizeof(uint)

            // Read thunk map
            if (reader.BytesRemaining < Header.NumberOfThunks * 4) // 4 = sizeof(uint)
                throw new Exception("Could not read a thunk map.");
            ThunkMap = reader.ReadUintArray((int)Header.NumberOfThunks);

            // Read sections
            if (reader.BytesRemaining < Header.NumberOfSections * PublicsStreamSectionOffset.Size)
                throw new Exception("Publics stream doesn't contain all sections specified in the header.");
            PublicsStreamSectionOffset[] sections = new PublicsStreamSectionOffset[Header.NumberOfSections];
            for (int i = 0; i < sections.Length; i++)
                sections[i] = PublicsStreamSectionOffset.Read(reader);
            Sections = sections;

            if (reader.BytesRemaining > 0)
                throw new Exception("Corrupted publics stream.");

            publicSymbolsCache = SimpleCache.CreateStruct(() =>
            {
                Public32Symbol[] publicSymbols = new Public32Symbol[GlobalsStream.Symbols.Count];

                for (int i = 0; i < publicSymbols.Length; i++)
                    publicSymbols[i] = GlobalsStream.Symbols[i] as Public32Symbol;
                return publicSymbols;
            });
        }

        /// <summary>
        /// Gets publics stream header.
        /// </summary>
        public PublicsStreamHeader Header { get; private set; }

        /// <summary>
        /// Gets the globals stream.
        /// </summary>
        public GlobalsStream GlobalsStream { get; private set; }

        /// <summary>
        /// Gets the address map.
        /// </summary>
        public uint[] AddressMap { get; private set; }

        /// <summary>
        /// Gets the thunk map.
        /// </summary>
        public uint[] ThunkMap { get; private set; }

        /// <summary>
        /// Gets the sections.
        /// </summary>
        public PublicsStreamSectionOffset[] Sections { get; private set; }

        /// <summary>
        /// Gets the list of public symbols.
        /// </summary>
        public IReadOnlyList<Public32Symbol> PublicSymbols => publicSymbolsCache.Value;
    }
}
