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
            }
        }

        private void ReadSymbolStream(SymbolStream symbolStream)
        {
            var references = symbolStream.References;
            for (int i = 0; i < references.Count; i++)
            {
                Assert.NotNull(symbolStream[i]);
                Assert.Equal(symbolStream[i], symbolStream.GetSymbolRecordByOffset(references[i].DataOffset - RecordPrefix.Size));
            }
            var kinds = references.Select(r => r.Kind).Distinct().ToArray();
            foreach (var kind in kinds)
                Assert.NotEmpty(symbolStream[kind]);
        }
    }
}
