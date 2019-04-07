#pragma warning disable 1591
using System;

namespace SharpPdb.Windows.SymbolRecords
{
    [Flags]
    public enum CompileSymbolFlags : uint
    {
        None = 0,
        SourceLanguageMask = 0xFF,
        EC = 1 << 8,
        NoDbgInfo = 1 << 9,
        LTCG = 1 << 10,
        NoDataAlign = 1 << 11,
        ManagedPresent = 1 << 12,
        SecurityChecks = 1 << 13,
        HotPatch = 1 << 14,
        CVTCIL = 1 << 15,
        MSILModule = 1 << 16,
        Sdl = 1 << 17,
        PGO = 1 << 18,
        Exp = 1 << 19,
    }
}
