using System;
using System.Collections.Generic;

namespace SharpPdb.Managed
{
    /// <summary>
    /// Represents PDB file reader.
    /// </summary>
    public interface IPdbFile : IDisposable
    {
        /// <summary>
        /// Gets the PDB file identifier.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Gets the PDB stamp.
        /// </summary>
        uint Stamp { get; }

        /// <summary>
        /// Gets the list of functions described in this PDB file.
        /// </summary>
        IReadOnlyList<IPdbFunction> Functions { get; }
    }
}
