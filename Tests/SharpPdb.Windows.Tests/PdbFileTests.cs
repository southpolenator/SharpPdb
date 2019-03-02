using SharpUtilities;
using System;
using System.Reflection;
using Xunit;

namespace SharpPdb.Windows.Tests
{
    public class PdbFileTests : TestBase
    {
        [Fact]
        public void Open()
        {
            using (PdbFile pdb = OpenPdb(1))
            {
            }

            using (PdbFile pdb = new PdbFile(new MemoryLoadedFile(GetPdbPath(1))))
            {
            }
        }

        [Fact]
        public void OpenFail()
        {
            Assert.ThrowsAny<Exception>(() => new PdbFile(Guid.NewGuid().ToString()));

            MemoryLoadedFile file = new MemoryLoadedFile(Assembly.GetExecutingAssembly().Location);
            Assert.ThrowsAny<Exception>(() => new PdbFile(file));
        }

        [Fact]
        public void Properties()
        {
            using (PdbFile pdb = OpenPdb(1))
            {
                Assert.NotNull(pdb.FreePageMap);
                Assert.Equal(531U, pdb.FreePageMapBitLength);
                Assert.NotNull(pdb.DbiStream);
                Assert.NotNull(pdb.InfoStream);
                Assert.NotNull(pdb.PdbSymbolStream);
                Assert.NotNull(pdb.TpiStream);
                Assert.NotNull(pdb.IpiStream);
                Assert.NotNull(pdb.Streams);
            }
        }
    }
}
