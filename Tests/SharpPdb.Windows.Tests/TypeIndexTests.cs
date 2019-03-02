using Xunit;

namespace SharpPdb.Windows.Tests
{
    public class TypeIndexTests
    {
        [Fact]
        public void BuiltinTypes()
        {
            var builtinTypes = TypeIndex.BuiltinTypes;
            foreach (var type in builtinTypes)
            {
                Assert.True(type.IsSimple);
                Assert.False(type.IsNoneType);
                Assert.NotNull(type.SimpleTypeName);
            }
        }

        [Fact]
        public void ArrayIndex()
        {
            TypeIndex typeIndex = TypeIndex.FromArrayIndex(10);

            Assert.False(typeIndex.IsSimple);
            Assert.False(typeIndex.IsNoneType);
            Assert.Equal(10U, typeIndex.ArrayIndex);
        }

        [Fact]
        public void SimpleTypeKindAndMode()
        {
            TypeIndex typeIndex = new TypeIndex(SimpleTypeKind.Byte, SimpleTypeMode.Direct);

            Assert.Equal(SimpleTypeKind.Byte, typeIndex.SimpleKind);
            Assert.Equal(SimpleTypeMode.Direct, typeIndex.SimpleMode);
        }

        [Fact]
        public void Equal()
        {
            TypeIndex ti10 = new TypeIndex(10);
            TypeIndex ti20 = new TypeIndex(20);

            Assert.False(ti10 == ti20);
            Assert.True(ti10 != ti20);
            Assert.False(ti10.Equals(ti20));
            Assert.Equal(10, ti10.GetHashCode());
        }

        [Fact]
        public void ToStringTest()
        {
            TypeIndex typeIndexByte = new TypeIndex(SimpleTypeKind.Byte, SimpleTypeMode.Direct);
            TypeIndex typeIndexArray10 = TypeIndex.FromArrayIndex(10);

            Assert.NotNull(typeIndexByte.ToString());
            Assert.NotNull(typeIndexArray10.ToString());
        }
    }
}
