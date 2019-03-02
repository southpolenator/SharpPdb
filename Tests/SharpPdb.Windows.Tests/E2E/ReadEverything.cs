using SharpPdb.Windows.SymbolRecords;
using SharpPdb.Windows.TPI;
using SharpPdb.Windows.TypeRecords;
using System.Linq;
using Xunit;

namespace SharpPdb.Windows.Tests.E2E
{
    public class ReadEverything : TestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Test1(int pdbIndex)
        {
            using (PdbFile pdb = OpenPdb(pdbIndex))
            {
                Assert.NotNull(pdb.PdbSymbolStream);
                ReadSymbolStream(pdb.PdbSymbolStream);
                Assert.NotNull(pdb.TpiStream);
                ReadTpiStream(pdb.TpiStream);
            }
        }

        private void ReadSymbolStream(SymbolStream symbolStream)
        {
            var references = symbolStream.References;
            Assert.NotNull(references);
            for (int i = 0; i < references.Count; i++)
            {
                Assert.NotNull(symbolStream[i]);
                Assert.Equal(symbolStream[i], symbolStream.GetSymbolRecordByOffset(references[i].DataOffset - RecordPrefix.Size));
            }
            var kinds = references.Select(r => r.Kind).Distinct().ToArray();
            foreach (var kind in kinds)
                Assert.NotEmpty(symbolStream[kind]);

            // Check symbol record kinds
            var allKinds = new[]
            {
                AttributeSlotSymbol.Kinds,
                BlockSymbol.Kinds,
                CoffGroupSymbol.Kinds,
                ConstantSymbol.Kinds,
                DataSymbol.Kinds,
                EndSymbol.Kinds,
                ManagedProcedureSymbol.Kinds,
                NamespaceSymbol.Kinds,
                OemSymbol.Kinds,
                ProcedureReferenceSymbol.Kinds,
                ProcedureSymbol.Kinds,
                Public32Symbol.Kinds,
                SectionSymbol.Kinds,
                ThreadLocalDataSymbol.Kinds,
                Thunk32Symbol.Kinds,
                TrampolineSymbol.Kinds,
                UdtSymbol.Kinds,
            };
            Assert.NotEmpty(allKinds.SelectMany(ka => ka.SelectMany(k => symbolStream[k])));
        }

        private void ReadTpiStream(TpiStream tpiStream)
        {
            if (tpiStream.HashSubstream != null)
            {
                Assert.NotNull(tpiStream.HashValues);
                Assert.NotEmpty(tpiStream.TypeIndexOffsets);
            }
            else
            {
                Assert.Null(tpiStream.HashValues);
                Assert.Null(tpiStream.TypeIndexOffsets);
            }
            if (tpiStream.HashSubstream != null && tpiStream.Header.HashAdjustersBuffer.Length > 0)
                Assert.NotNull(tpiStream.HashAdjusters);
            else
                Assert.Null(tpiStream.HashAdjusters);
            var references = tpiStream.References;
            Assert.NotNull(references);
            Assert.Equal(references.Count, tpiStream.TypeRecordCount);
            for (int i = 0; i < references.Count; i++)
                Assert.NotNull(tpiStream[TypeIndex.FromArrayIndex(i)]);
            var kinds = references.Select(r => r.Kind).Distinct().ToArray();
            foreach (var kind in kinds)
            {
                Assert.NotEmpty(tpiStream.GetIndexes(kind));
                Assert.NotEmpty(tpiStream[kind]);
            }

            // Check type record kinds
            var allKinds = new[]
            {
                ArgumentListRecord.Kinds,
                ArrayRecord.Kinds,
                BaseClassRecord.Kinds,
                BitFieldRecord.Kinds,
                BuildInfoRecord.Kinds,
                ClassRecord.Kinds,
                DataMemberRecord.Kinds,
                EnumeratorRecord.Kinds,
                EnumRecord.Kinds,
                FieldListRecord.Kinds,
                FunctionIdRecord.Kinds,
                LabelRecord.Kinds,
                ListContinuationRecord.Kinds,
                MemberFunctionIdRecord.Kinds,
                MemberFunctionRecord.Kinds,
                MethodOverloadListRecord.Kinds,
                ModifierRecord.Kinds,
                NestedTypeRecord.Kinds,
                OneMethodRecord.Kinds,
                OverloadedMethodRecord.Kinds,
                PointerRecord.Kinds,
                StaticDataMemberRecord.Kinds,
                StringIdRecord.Kinds,
                StringListRecord.Kinds,
                UdtModuleSourceLineRecord.Kinds,
                UnionRecord.Kinds,
                VirtualBaseClassRecord.Kinds,
                VirtualFunctionPointerRecord.Kinds,
                VirtualFunctionTableShapeRecord.Kinds,
            };
            if (references.Count > 0)
                Assert.NotEmpty(allKinds.SelectMany(ka => ka.SelectMany(k => tpiStream[k])));
        }
    }
}
