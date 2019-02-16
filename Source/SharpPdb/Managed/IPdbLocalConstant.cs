namespace SharpPdb.Managed
{
    /// <summary>
    /// Represents local constant defined in local scope in PDB file.
    /// </summary>
    public interface IPdbLocalConstant
    {
        /// <summary>
        /// Gets the name of the local constant.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value of the local constant.
        /// </summary>
        object Value { get; }
    }
}
