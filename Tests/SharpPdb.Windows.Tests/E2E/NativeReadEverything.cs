using SharpPdb.Windows.TPI;
using System.Linq;
using Xunit;

namespace SharpPdb.Windows.Tests.E2E
{
    public class NativeReadEverything : TestBase
    {
        [Fact]
        public void Test1()
        {
            using (PdbFile pdb = OpenPdb(1))
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
        }

        private void ReadTpiStream(TpiStream tpiStream)
        {
            Assert.NotNull(tpiStream.HashValues);
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
            Assert.NotEmpty(tpiStream.TypeIndexOffsets);
        }
    }
}
