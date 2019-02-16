using System;

namespace SharpPdb.Managed
{
    /// <summary>
    /// Represents source file referenced in PDB file.
    /// </summary>
    public interface IPdbSource
    {
        /// <summary>
        /// Gets the hash value bytes.
        /// </summary>
        byte[] Hash { get; }

        /// <summary>
        /// Gets the hash algorithm id.
        /// </summary>
        Guid HashAlgorithm { get; }

        /// <summary>
        /// Gets the language id.
        /// </summary>
        Guid Language { get; }

        /// <summary>
        /// Gets the source file name.
        /// </summary>
        string Name { get; }
    }
}
