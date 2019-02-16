using System.Collections.Generic;

namespace SharpPdb.Managed
{
    /// <summary>
    /// Represents function referenced in PDB file.
    /// </summary>
    public interface IPdbFunction
    {
        /// <summary>
        /// Gets the method token.
        /// </summary>
        int Token { get; }

        /// <summary>
        /// Gets the list of local scopes in this function.
        /// </summary>
        IReadOnlyList<IPdbLocalScope> LocalScopes { get; }

        /// <summary>
        /// Gets the list of sequence points in this function.
        /// </summary>
        IReadOnlyList<IPdbSequencePoint> SequencePoints { get; }
    }
}
