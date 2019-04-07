#pragma warning disable 1591
using System;

namespace SharpPdb.Windows.SymbolRecords
{
    [Flags]
    public enum ExportFlags : ushort
    {
        None = 0,
        IsConstant = 0x0001,
        IsData = 0x0002,
        IsPrivate = 0x0004,
        HasNoName = 0x0008,
        HasExplicitOrdinal = 0x0010,
        IsForwarder = 0x0020,
    }
}
