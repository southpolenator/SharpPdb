#pragma warning disable 1591
namespace SharpPdb.Windows.SymbolRecords
{
    public enum FrameCookieKind : byte
    {
        Copy,
        XorStackPointer,
        XorFramePointer,
        XorR13,
    }
}
