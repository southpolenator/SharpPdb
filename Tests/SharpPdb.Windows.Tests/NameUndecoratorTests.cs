using SharpPdb.Common.Tests;
using Xunit;

namespace SharpPdb.Windows.Tests
{
    public class NameUndecoratorTests : TestBase
    {
        [Fact]
        public void Destructor()
        {
            Test("public: __cdecl TestClass::~TestClass(void) __ptr64", "??1TestClass@@QEAA@XZ");
            Test("public: virtual void * __ptr64 __cdecl TestInterface::`scalar deleting destructor'(unsigned int) __ptr64", "??_GTestInterface@@UEAAPEAXI@Z");
            Test("public: virtual __cdecl TestInterface2::~TestInterface2(void) __ptr64", "??1TestInterface2@@UEAA@XZ");
        }

        [Fact]
        public void VirtualTable()
        {
            Test("const TestInterface::`vftable'", "??_7TestInterface@@6B@");
            Test("const TemplateTest<class TestClass2>::`vftable'{for `TestClass'}", "??_7?$TemplateTest@VTestClass2@@@@6BTestClass@@@");
            Test("const TestClass2::`vbtable'", "??_8TestClass2@@7B@");
            Test("const TestClass3::`vbtable'{for `TestClass4'}", "??_8TestClass3@@7BTestClass4@@@");
        }

        [Fact]
        public void VirtualCall()
        {
            Test("[thunk]: __cdecl TestClass::`vcall'{8,{flat}}' }'", "??_9TestClass@@$B7AA");
        }

        private void Test(string expectedOutput, string input, NameUndecorator.Flags flags = NameUndecorator.Flags.Complete)
        {
            Assert.Equal(expectedOutput, NameUndecorator.UnDecorateSymbolName(input, flags));
        }
    }
}
