namespace SharpPdb.Managed
{
    /// <summary>
    /// Represents local variable defined in local scope in PDB file.
    /// </summary>
    public interface IPdbLocalVariable
    {
        /// <summary>
        /// Gets the index on the stack of the local variable.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the name of the local variable.
        /// </summary>
        string Name { get; }
    }
}
