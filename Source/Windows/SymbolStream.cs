using SharpPdb.Windows.SymbolRecords;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpPdb.Windows
{
    /// <summary>
    /// Represents PDB symbol stream.
    /// </summary>
    public class SymbolStream
    {
        /// <summary>
        /// List of all symbol references in this stream.
        /// </summary>
        private List<SymbolRecordReference> references = new List<SymbolRecordReference>();

        /// <summary>
        /// Dictionary of reference indexes by position in the binary reader.
        /// </summary>
        private Dictionary<long, int> referenceIndexByOffset = new Dictionary<long, int>();

        /// <summary>
        /// Array cache of symbols in this symbol stream.
        /// </summary>
        private ArrayCache<SymbolRecord> symbols;

        /// <summary>
        /// Dictionary cache of symbols by its kind.
        /// </summary>
        private DictionaryCache<SymbolRecordKind, SymbolRecord[]> symbolsByKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolStream"/> class.
        /// </summary>
        /// <param name="stream">PDB symbol stream.</param>
        public SymbolStream(PdbStream stream)
            : this(stream.Reader)
        {
            Stream = stream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolStream"/> class.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        /// <param name="end">End of the symbol stream in binary reader. If it is less than 0 or bigger than binary reader length, it will be read fully.</param>
        public SymbolStream(IBinaryReader reader, long end = -1)
        {
            Reader = reader;

            long position = reader.Position;
            if (end < 0 || end > reader.Length)
                end = reader.Length;

            while (position < end)
            {
                RecordPrefix prefix = RecordPrefix.Read(reader);

                if (prefix.RecordLength < 2)
                    throw new Exception("CV corrupt record");

                SymbolRecordKind kind = (SymbolRecordKind)prefix.RecordKind;
                ushort dataLen = prefix.DataLen;

                referenceIndexByOffset.Add(position, references.Count);
                references.Add(new SymbolRecordReference
                {
                    DataOffset = (uint)position + RecordPrefix.Size,
                    Kind = kind,
                    DataLen = dataLen,
                });
                position += dataLen + RecordPrefix.Size;
                reader.Move(dataLen);
            }

            symbolsByKind = new DictionaryCache<SymbolRecordKind, SymbolRecord[]>(GetSymbolsByKind);
            symbols = new ArrayCache<SymbolRecord>(references.Count, GetSymbol);
        }

        /// <summary>
        /// Gets the PDB stream.
        /// </summary>
        public PdbStream Stream { get; private set; }

        /// <summary>
        /// Gets the stream binary reader.
        /// </summary>
        public IBinaryReader Reader { get; private set; }

        /// <summary>
        /// Gets the read-only list of all symbol references in this stream.
        /// </summary>
        public IReadOnlyList<SymbolRecordReference> References => references;

        /// <summary>
        /// Indexing operator for getting symbol record at the given index.
        /// </summary>
        /// <param name="index">Index of the symbol record.</param>
        /// <returns>Symbol record at the given index position.</returns>
        public SymbolRecord this[int index] => symbols[index];

        /// <summary>
        /// Indexing operator for getting all symbols of the given kind.
        /// </summary>
        /// <param name="kind">Symbol record kind that should be parsed from this symbol stream.</param>
        /// <returns>Array of symbol record for the specified symbol record kind.</returns>
        public SymbolRecord[] this[SymbolRecordKind kind] => symbolsByKind[kind];

        /// <summary>
        /// Gets symbol record by offset in the binary reder stream.
        /// </summary>
        /// <param name="position">Position in the binary reader.</param>
        /// <returns>Symbol record at the specified position.</returns>
        public SymbolRecord GetSymbolRecordByOffset(long position)
        {
            int index = referenceIndexByOffset[position];

            return symbols[index];
        }

        /// <summary>
        /// Tries to get symbol record by offset in the binary reder stream.
        /// </summary>
        /// <param name="position">Position in the binary reader.</param>
        /// <param name="symbolRecord">Symbol record at the specified position.</param>
        /// <returns><c>true</c> if offset points to symbol record; <c>false</c> otherwise</returns>
        public bool TryGetSymbolRecordByOffset(long position, out SymbolRecord symbolRecord)
        {
            if (referenceIndexByOffset.TryGetValue(position, out int index))
            {
                symbolRecord = symbols[index];
                return true;
            }
            symbolRecord = null;
            return false;
        }

        /// <summary>
        /// Parses all symbols of the specified symbol record kind.
        /// </summary>
        /// <param name="kind">Symbol record kind.</param>
        /// <returns>Array of symbol record for the specified symbol record kind.</returns>
        private SymbolRecord[] GetSymbolsByKind(SymbolRecordKind kind)
        {
            List<SymbolRecord> symbols = new List<SymbolRecord>();

            for (int i = 0; i < references.Count; i++)
                if (references[i].Kind == kind)
                {
                    SymbolRecord symbol = this.symbols[i];

                    if (symbol != null)
                        symbols.Add(symbol);
                }
            return symbols.ToArray();
        }

        /// <summary>
        /// Reads symbol record from symbol references for the specified index.
        /// </summary>
        /// <param name="index">Index of the symbol record.</param>
        private SymbolRecord GetSymbol(int index)
        {
            // Since DictionaryCache is allowing only single thread to call this function, we don't need to lock reader here.
            SymbolRecordReference reference = references[index];

            Reader.Position = reference.DataOffset;
            switch (reference.Kind)
            {
                case SymbolRecordKind.S_GPROC32:
                case SymbolRecordKind.S_LPROC32:
                case SymbolRecordKind.S_GPROC32_ID:
                case SymbolRecordKind.S_LPROC32_ID:
                case SymbolRecordKind.S_LPROC32_DPC:
                case SymbolRecordKind.S_LPROC32_DPC_ID:
                    return ProcedureSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_PUB32:
                    return Public32Symbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_CONSTANT:
                case SymbolRecordKind.S_MANCONSTANT:
                    return ConstantSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_LDATA32:
                case SymbolRecordKind.S_GDATA32:
                case SymbolRecordKind.S_LMANDATA:
                case SymbolRecordKind.S_GMANDATA:
                    return DataSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_PROCREF:
                case SymbolRecordKind.S_LPROCREF:
                    return ProcedureReferenceSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_TOKENREF:
                    return TokenReferenceSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_UDT:
                case SymbolRecordKind.S_COBOLUDT:
                    return UdtSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_LTHREAD32:
                case SymbolRecordKind.S_GTHREAD32:
                    return ThreadLocalDataSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_GMANPROC:
                case SymbolRecordKind.S_LMANPROC:
                    return ManagedProcedureSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_BLOCK32:
                    return BlockSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_OEM:
                    return OemSymbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                case SymbolRecordKind.S_UNAMESPACE:
                    return NamespaceSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_MANSLOT:
                    return AttributeSlotSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_END:
                case SymbolRecordKind.S_INLINESITE_END:
                    return EndSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_ANNOTATION:
                    return AnnotationSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_ANNOTATIONREF:
                    return AnnotationReferenceSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_REGREL32:
                    return RegisterRelativeSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_OBJNAME:
                    return ObjectNameSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_COMPILE2:
                    return Compile2Symbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_COMPILE3:
                    return Compile3Symbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_ENVBLOCK:
                    return EnvironmentBlockSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_BUILDINFO:
                    return BuildInfoSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_FRAMEPROC:
                    return FrameProcedureSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_LABEL32:
                    return LabelSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_HEAPALLOCSITE:
                    return HeapAllocationSiteSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_CALLSITEINFO:
                    return CallSiteInfoSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_FRAMECOOKIE:
                    return FrameCookieSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_THUNK32:
                    return Thunk32Symbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                case SymbolRecordKind.S_LOCAL:
                    return LocalSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_DEFRANGE_REGISTER:
                    return DefRangeRegisterSymbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                case SymbolRecordKind.S_DEFRANGE_REGISTER_REL:
                    return DefRangeRegisterRelativeSymbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                case SymbolRecordKind.S_DEFRANGE_SUBFIELD_REGISTER:
                    return DefRangeSubfieldRegisterSymbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                case SymbolRecordKind.S_DEFRANGE_FRAMEPOINTER_REL:
                    return DefRangeFramePointerRelativeSymbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                case SymbolRecordKind.S_DEFRANGE_FRAMEPOINTER_REL_FULL_SCOPE:
                    return DefRangeFramePointerRelativeFullScopeSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_CALLEES:
                case SymbolRecordKind.S_CALLERS:
                    return FunctionListSymbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                case SymbolRecordKind.S_FILESTATIC:
                    return FileStaticSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_TRAMPOLINE:
                    return TrampolineSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_SECTION:
                    return SectionSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_COFFGROUP:
                    return CoffGroupSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_EXPORT:
                    return ExportSymbol.Read(Reader, this, index, reference.Kind);
                case SymbolRecordKind.S_INLINESITE:
                    return InlineSiteSymbol.Read(Reader, this, index, reference.Kind, reference.DataLen);
                default:
#if DEBUG
                    throw new NotImplementedException($"Unknown reference kind: {reference.Kind}");
#else
                    return null;
#endif
            }
        }
    }
}
