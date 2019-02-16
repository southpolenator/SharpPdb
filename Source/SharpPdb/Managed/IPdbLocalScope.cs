using System.Collections.Generic;

namespace SharpPdb.Managed
{
    /// <summary>
    /// Represents local scope in PDB file.
    /// </summary>
    public interface IPdbLocalScope
    {
        /// <summary>
        /// Gets the IL offset of the local scope start.
        /// </summary>
        int StartOffset { get; }

        /// <summary>
        /// Gets the IL offset of the local scope end.
        /// </summary>
        int EndOffset { get; }

        /// <summary>
        /// Gets the length of the local scope.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the function that contains this local scope.
        /// </summary>
        IPdbFunction Function { get; }

        /// <summary>
        /// Gets the parent local scope or <c>null</c> if it doesn't exist.
        /// </summary>
        IPdbLocalScope Parent { get; }

        /// <summary>
        /// Gets the list of contained local scopes.
        /// </summary>
        IReadOnlyList<IPdbLocalScope> Children { get; }

        /// <summary>
        /// Gets the list of constants declared in this local scope.
        /// </summary>
        IReadOnlyList<IPdbLocalConstant> Constants { get; }

        /// <summary>
        /// Gets the list of variables declared in this local scope.
        /// </summary>
        IReadOnlyList<IPdbLocalVariable> Variables { get; }
    }
}
