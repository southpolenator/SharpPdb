using SharpUtilities;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents compile3 symbol from the symbols stream.
    /// </summary>
    public class Compile3Symbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_COMPILE3,
        };

        /// <summary>
        /// Gets the compile symbol flags.
        /// </summary>
        public CompileSymbolFlags Flags { get; private set; }

        /// <summary>
        /// Gets the machine type.
        /// </summary>
        public CpuType Machine { get; private set; }

        /// <summary>
        /// Gets the frontend major version.
        /// </summary>
        public ushort VersionFrontendMajor { get; private set; }

        /// <summary>
        /// Gets the frontend minor version.
        /// </summary>
        public ushort VersionFrontendMinor { get; private set; }

        /// <summary>
        /// Gets the frontend build version.
        /// </summary>
        public ushort VersionFrontendBuild { get; private set; }

        /// <summary>
        /// Gets the frontend QFE number of version.
        /// </summary>
        public ushort VersionFrontendQFE { get; private set; }

        /// <summary>
        /// Gets the backend major version.
        /// </summary>
        public ushort VersionBackendMajor { get; private set; }

        /// <summary>
        /// Gets the backend minor version.
        /// </summary>
        public ushort VersionBackendMinor { get; private set; }

        /// <summary>
        /// Gets the backend build version.
        /// </summary>
        public ushort VersionBackendBuild { get; private set; }

        /// <summary>
        /// Gets the backend QFE number of version.
        /// </summary>
        public ushort VersionBackendQFE { get; private set; }

        /// <summary>
        /// Gets the version string.
        /// </summary>
        public StringReference Version;

        /// <summary>
        /// Reads <see cref="Compile3Symbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="symbolStream">Symbol stream that contains this symbol record.</param>
        /// <param name="symbolStreamIndex">Index in symbol stream <see cref="SymbolStream.References"/> array.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static Compile3Symbol Read(IBinaryReader reader, SymbolStream symbolStream, int symbolStreamIndex, SymbolRecordKind kind)
        {
            return new Compile3Symbol
            {
                SymbolStream = symbolStream,
                SymbolStreamIndex = symbolStreamIndex,
                Kind = kind,
                Flags = (CompileSymbolFlags)reader.ReadUint(),
                Machine = (CpuType)reader.ReadUshort(),
                VersionFrontendMajor = reader.ReadUshort(),
                VersionFrontendMinor = reader.ReadUshort(),
                VersionFrontendBuild = reader.ReadUshort(),
                VersionFrontendQFE = reader.ReadUshort(),
                VersionBackendMajor = reader.ReadUshort(),
                VersionBackendMinor = reader.ReadUshort(),
                VersionBackendBuild = reader.ReadUshort(),
                VersionBackendQFE = reader.ReadUshort(),
                Version = reader.ReadCString(),
            };
        }
    }
}
