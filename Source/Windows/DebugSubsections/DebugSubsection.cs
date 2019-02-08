namespace SharpPdb.Windows.DebugSubsections
{
    /// <summary>
    /// Base class for all debug subsection classes.
    /// </summary>
    public class DebugSubsection
    {
        /// <summary>
        /// Type of the debug subsection.
        /// </summary>
        public DebugSubsectionKind Kind { get; protected set; }
    }
}
