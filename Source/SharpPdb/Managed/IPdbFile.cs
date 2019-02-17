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
        /// Gets the PDB file age.
        /// </summary>
        int Age { get; }

        /// <summary>
        /// Gets the list of functions described in this PDB file.
        /// </summary>
        IReadOnlyList<IPdbFunction> Functions { get; }

        /// <summary>
        /// Find function by the specified token.
        /// </summary>
        /// <param name="token">Method definition token.</param>
        /// <returns><see cref="IPdbFunction"/> object if found, <c>null</c> otherwise.</returns>
        IPdbFunction GetFunctionFromToken(int token);
    }
}
