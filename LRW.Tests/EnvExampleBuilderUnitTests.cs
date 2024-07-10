using FakeItEasy;
using LRW.Configuration;
using System.Reflection;

namespace LRW.Tests
{
    public class EnvExampleBuilderUnitTests
    {
        public class FakeKey1() : Key("FAKE_KEY_ONE", "1", ["This is a fake key number 1"]);
        public class FakeKey2() : Key("FAKE_KEY_TWO", "2", ["This is a fake key number 2", "And key number 2 has another documentation"]);

        [Fact]
        public void BuildUnix_EnvExampleBuilder_ReturnsCorrectStringForEnvExampleFileContent()
        {
            //Arrange
            var assembly = A.Fake<Assembly>();
            var expectedText = "# Automatically generated file, do not edit.\n\n"
                            + "# This is a fake key number 1\n"
                            + "FAKE_KEY_ONE=1\n\n"
                            + "# This is a fake key number 2\n"
                            + "# And key number 2 has another documentation\n"
                            + "FAKE_KEY_TWO=2\n\n";

            var t1 = typeof(FakeKey1).GetTypeInfo();
            var t2 = typeof(FakeKey2).GetTypeInfo();

            A.CallTo(() => assembly.DefinedTypes).Returns([t1, t2]);

            //Act
            var env = EnvExampleBuilder.BuildUnix(assembly);

            //Assert
            Assert.Equal(expectedText, env);
        }
    }
}
