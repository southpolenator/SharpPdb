#pragma warning disable 1591

namespace SharpPdb.Windows
{
    public enum PdbFeatureSignature : uint
    {
        VC110 = 20091201,
        VC140 = 20140508,
        NoTypeMerge = 0x4D544F4E,
        MinimalDebugInfo = 0x494E494D,
    }
}
