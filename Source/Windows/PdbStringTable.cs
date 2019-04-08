using SharpPdb.Windows.Utility;
using SharpUtilities;
using System.Collections.Generic;

namespace SharpPdb.Windows
{
    /// <summary>
    /// String table that can appear in PDB file.
    /// </summary>
    public class PdbStringTable
    {
        /// <summary>
        /// Cache of read strings for <see cref="StringsStream"/> specified offset.
        /// </summary>
        private DictionaryCache<uint, string> stringsCache;

        /// <summary>
        /// Cache of all strings in the strings table.
        /// </summary>
        private SimpleCacheStruct<Dictionary<uint, string>> dictionaryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbStringTable"/> class.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public PdbStringTable(IBinaryReader reader)
        {
            // Read header
            Signature = reader.ReadUint();
            HashVersion = reader.ReadUint();
            StringsStreamSize = reader.ReadUint();

            // Read strings table
            StringsStream = reader.ReadSubstream(StringsStreamSize);
            stringsCache = new DictionaryCache<uint, string>((uint offset) =>
            {
                StringsStream.Position = offset;
                return StringsStream.ReadCString().String;
            });

            // Read table of offsets that can be accessed by hash function
            uint offsetsCount = reader.ReadUint();

            Offsets = reader.ReadUintArray((int)offsetsCount);

            // Read epilogue
            StringsCount = reader.ReadInt();

            dictionaryCache = SimpleCache.CreateStruct(() =>
            {
                Dictionary<uint, string> strings = new Dictionary<uint, string>();

                foreach (uint offset in Offsets)
                    strings[offset] = stringsCache[offset];
                return strings;
            });
        }

        /// <summary>
        /// String table signature.
        /// </summary>
        public uint Signature { get; private set; }

        /// <summary>
        /// Version of the hash function to be used for searching strings.
        /// </summary>
        public uint HashVersion { get; private set; }

        /// <summary>
        /// Length of the <see cref="StringsStream"/> in bytes.
        /// </summary>
        public uint StringsStreamSize { get; private set; }

        /// <summary>
        /// Strings stream binary reader.
        /// </summary>
        public IBinaryReader StringsStream { get; private set; }

        /// <summary>
        /// Number of strings in this string table.
        /// </summary>
        public int StringsCount { get; private set; }

        /// <summary>
        /// Offsets of strings in <see cref="StringsStream"/>.
        /// </summary>
        public uint[] Offsets { get; private set; }

        /// <summary>
        /// Gets all strings in the strings table.
        /// </summary>
        public Dictionary<uint, string> Dictionary => dictionaryCache.Value;

        /// <summary>
        /// Returns string at the specified index.
        /// </summary>
        /// <param name="index">Index of the string in this string table.</param>
        public string this[int index] => stringsCache[Offsets[index]];

        /// <summary>
        /// Gets string at the specified offset.
        /// </summary>
        /// <param name="offset">Offset where string is stored in the stream.</param>
        public string GetString(uint offset)
        {
            return stringsCache[offset];
        }
    }
}
