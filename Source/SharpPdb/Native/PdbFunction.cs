using System.Collections.Generic;
using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents function read from the PDB file.
    /// </summary>
    public class PdbFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFunction"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="procedure">Procedure symbol record.</param>
        internal PdbFunction(PdbFileReader pdb, ProcedureSymbol procedure)
        {
            Pdb = pdb;
            Procedure = procedure;
        }

        /// <summary>
        /// Gets the PDB File reader.
        /// </summary>
        public PdbFileReader Pdb { get; private set; }

        /// <summary>
        /// Gets the procedure symbol record.
        /// </summary>
        public ProcedureSymbol Procedure { get; private set; }

        /// <summary>
        /// Used in local procedures, global procedures, thunk start, with start, and
        /// block start symbols. If the scope is not enclosed by another lexical scope,
        /// then <see cref="ParentOffset"/> is zero. Otherwise, the parent of this scope is the symbol
        /// within this module that opens the outer scope that encloses this scope but
        /// encloses no other scope that encloses this scope. The <see cref="ParentOffset"/> field contains
        /// the offset from the beginning of the module's symbol table of the symbol
        /// that opens the enclosing lexical scope.
        /// </summary>
        public uint ParentOffset => Procedure.ParentOffset;

        /// <summary>
        /// Used in start search local procedures, global procedures, and thunk start
        /// symbols. The <see cref="Next"/> field, along with the start search symbol, defines a
        /// group of lexically scoped symbols within a symbol table that is contained
        /// within a code segment or PE section. For each segment or section
        /// represented in the symbol table, there is a start search symbol that contains
        /// the offset from the start of the symbols for this module to the first procedure
        /// or thunk contained in the segment. Each outermost lexical scope symbol
        /// has a next field containing the next outermost scope symbol contained in the
        /// segment. The last outermost scope in the symbol table for each segment has
        /// a next field of zero.
        /// </summary>
        public uint End => Procedure.End;

        /// <summary>
        /// This field is defined for local procedures, global procedures, thunk, block,
        /// and with symbols.The end field contains the offset from the start of the
        /// symbols for this module to the matching block end symbol that terminates
        /// the lexical scope.
        /// </summary>
        public uint Next => Procedure.Next;

        /// <summary>
        /// Gets the length in bytes of this procedure.
        /// </summary>
        public uint CodeSize => Procedure.CodeSize;

        /// <summary>
        /// Gets the offset in bytes from the start of the procedure to the point where the
        /// stack frame has been set up. Parameter and frame variables can be viewed at this point.
        /// </summary>
        public uint DebugStart => Procedure.DebugStart;

        /// <summary>
        /// Gets the offset in bytes from the start of the procedure to the point where the
        /// procedure is ready to return and has calculated its return value, if any.
        /// Frame and register variables can still be viewed.
        /// </summary>
        public uint DebugEnd => Procedure.DebugEnd;

        /// <summary>
        /// Gets the function type.
        /// </summary>
        public PdbType FunctionType => Pdb[Procedure.FunctionType];

        /// <summary>
        /// Gets the offset portion of the procedure address.
        /// </summary>
        public uint Offset => Procedure.Offset;

        /// <summary>
        /// Gets the segment portion of the procedure address.
        /// </summary>
        public ushort Segment => Procedure.Segment;

        /// <summary>
        /// Gets the procedure flags.
        /// </summary>
        public ProcedureFlags Flags => Procedure.Flags;

        /// <summary>
        /// Gets the name of procedure.
        /// </summary>
        public string Name => Procedure.Name.String;

        /// <summary>
        /// Gets the relative virtual address of this static field from module load address.
        /// </summary>
        public ulong RelativeVirtualAddress => Pdb.PdbFile.FindRelativeVirtualAddress(Segment, Offset);

        /// <summary>
        /// <c>true</c> if the function uses a custom calling convention.
        /// </summary>
        public bool HasCustomCallingConvention => (Flags & ProcedureFlags.HasCustomCallingConv) != 0;

        /// <summary>
        /// <c>true</c> if the function performs a far return.
        /// </summary>
        public bool HasFarReturn => (Flags & ProcedureFlags.HasFRET) != 0;

        /// <summary>
        /// <c>true</c> if the code has debug information for optimized code.
        /// </summary>
        public bool HasOptimizedDebugInfo => (Flags & ProcedureFlags.HasOptimizedDebugInfo) != 0;

        /// <summary>
        /// <c>true</c> if the function has frame pointer present.
        /// </summary>
        public bool HasFramePointer => (Flags & ProcedureFlags.HasFramePointer) != 0;

        /// <summary>
        /// <c>true</c> if the function is not an inline function.
        /// </summary>
        public bool IsNoInline => (Flags & ProcedureFlags.IsNoInline) != 0;

        /// <summary>
        /// <c>true</c> if the function does not return a value.
        /// </summary>
        public bool IsNoReturn => (Flags & ProcedureFlags.IsNoReturn) != 0;

        /// <summary>
        /// <c>true</c> if the function is not reachable.
        /// </summary>
        public bool IsUnreachable => (Flags & ProcedureFlags.IsUnreachable) != 0;

        /// <summary>
        /// <c>true</c> if the function has a return from interrupt.
        /// </summary>
        public bool HasInterruptReturn => (Flags & ProcedureFlags.HasIRET) != 0;

        /// <summary>
        /// Gets the part or all of the undecorated symbol name.
        /// </summary>
        /// <param name="flags">Specifies a combination of flags that control what is returned.</param>
        /// <returns>Returns the undecorated name for a C++ decorated name.</returns>
        public string GetUndecoratedName(NameUndecorator.Flags flags = NameUndecorator.Flags.Complete)
        {
            return NameUndecorator.UnDecorateSymbolName(Name, flags);
        }
    }
}
