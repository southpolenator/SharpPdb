using SharpPdb.Windows.Utility;

namespace SharpPdb.Windows.DebugSubsections
{
    /// <summary>
    /// Represents parsed CV_FileCheckSum structure.
    /// </summary>
    public class FileChecksumSubsection : DebugSubsection
    {
        /// <summary>
        /// Array of <see cref="DebugSubsectionKind"/> that this class can read.
        /// </summary>
        public static readonly DebugSubsectionKind[] Kinds = new DebugSubsectionKind[]
        {
            DebugSubsectionKind.FileChecksums,
        };

        /// <summary>
        /// Gets the index of file name in <code>/names</code> stream.
        /// </summary>
        public uint NameIndex { get; private set; }

        /// <summary>
        /// Gets the hash type.
        /// </summary>
        public FileChecksumHashType HashType { get; private set; }

        /// <summary>
        /// Gets the hash data binary reader.
        /// </summary>
        public IBinaryReader HashReader { get; private set; }

        /// <summary>
        /// Reads <see cref="FileChecksumSubsection"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Debug subsection kind.</param>
        public static FileChecksumSubsection Read(IBinaryReader reader, DebugSubsectionKind kind)
        {
            uint nameIndex = reader.ReadUint();
            byte hashLength = reader.ReadByte();
            FileChecksumHashType hashType = (FileChecksumHashType)reader.ReadByte();

            reader.Align(4);
            return new FileChecksumSubsection
            {
                Kind = kind,
                NameIndex = nameIndex,
                HashType = hashType,
                HashReader = reader.ReadSubstream(hashLength),
            };
        }
    }
}
