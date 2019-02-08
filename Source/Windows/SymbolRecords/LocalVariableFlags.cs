using System;

namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Represents local variable flags.
    /// </summary>
    [Flags]
    public enum LocalVariableFlags : ushort
    {
        /// <summary>
        /// Variable is a parameter.
        /// </summary>
        IsParam = 0x0001,

        /// <summary>
        /// Address is taken
        /// </summary>
        IsAddressTaken = 0x0002,

        /// <summary>
        /// Variable is compiler generated
        /// </summary>
        IsCompilerGenerated = 0x0004,

        /// <summary>
        /// The symbol is splitted in temporaries, which are treated by compiler as
        /// independent entities.
        /// </summary>
        IsAggregate = 0x0008,

        /// <summary>
        /// Counterpart of <see cref="IsAggregate"/> - tells that it is a part of a <see cref="IsAggregate"/> symbol.
        /// </summary>
        IsAggregated = 0x0010,

        /// <summary>
        /// Variable has multiple simultaneous lifetimes
        /// </summary>
        IsAliased = 0x0020,

        /// <summary>
        /// Represents one of the multiple simultaneous lifetimes
        /// </summary>
        IsAlias = 0x0040,
    }
}
