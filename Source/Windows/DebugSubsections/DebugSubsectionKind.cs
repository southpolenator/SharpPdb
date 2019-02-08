#pragma warning disable 1591

namespace SharpPdb.Windows.DebugSubsections
{
    /// <summary>
    /// Type of the <see cref="DebugSubsection"/>.
    /// </summary>
    public enum DebugSubsectionKind : uint
    {
        None = 0,
        Symbols = 0xf1,
        Lines = 0xf2,
        StringTable = 0xf3,
        FileChecksums = 0xf4,
        FrameData = 0xf5,
        InlineeLines = 0xf6,
        CrossScopeImports = 0xf7,
        CrossScopeExports = 0xf8,

        // These appear to relate to .Net assembly info.
        ILLines = 0xf9,
        FuncMDTokenMap = 0xfa,
        TypeMDTokenMap = 0xfb,
        MergedAssemblyInput = 0xfc,

        CoffSymbolRVA = 0xfd,
    }
}
