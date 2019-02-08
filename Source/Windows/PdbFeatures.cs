#pragma warning disable 1591

namespace SharpPdb.Windows
{
    public enum PdbFeatures : uint
    {
        None = 0x0,
        ContainsIdStream = 0x1,
        MinimalDebugInfo = 0x2,
        NoTypeMerging = 0x4,
    }
}
