using System;

namespace SharpPdb.Windows.DebugSubsections
{
    /// <summary>
    /// Represents lines subsection flags (data present).
    /// </summary>
    [Flags]
    public enum LinesSubsectionFlags : ushort
    {
        /// <summary>
        /// Lines data contains columns information.
        /// </summary>
        LinesHaveColumns = 0x0001,
    }
}
