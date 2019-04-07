#pragma warning disable 1591
using System;

namespace SharpPdb.Windows.SymbolRecords
{
    [Flags]
    public enum FrameProcedureOptions : uint
    {
        None = 0x00000000,
        HasAlloca = 0x00000001,
        HasSetJmp = 0x00000002,
        HasLongJmp = 0x00000004,
        HasInlineAssembly = 0x00000008,
        HasExceptionHandling = 0x00000010,
        MarkedInline = 0x00000020,
        HasStructuredExceptionHandling = 0x00000040,
        Naked = 0x00000080,
        SecurityChecks = 0x00000100,
        AsynchronousExceptionHandling = 0x00000200,
        NoStackOrderingForSecurityChecks = 0x00000400,
        Inlined = 0x00000800,
        StrictSecurityChecks = 0x00001000,
        SafeBuffers = 0x00002000,
        EncodedLocalBasePointerMask = 0x0000C000,
        EncodedParamBasePointerMask = 0x00030000,
        ProfileGuidedOptimization = 0x00040000,
        ValidProfileCounts = 0x00080000,
        OptimizedForSpeed = 0x00100000,
        GuardCfg = 0x00200000,
        GuardCfw = 0x00400000
    }
}
