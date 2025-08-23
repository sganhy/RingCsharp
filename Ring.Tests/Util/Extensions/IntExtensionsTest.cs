using Ring.Util.Enums;
using Ring.Util.Extensions;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace Ring.Tests.Util.Extensions;

public sealed class IntExtensionsTest : BaseTest
{
    public IntExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    public void ToXmlSchemaAttributeType_AllExistingEnumId_Enum()
    {
        // arrange 
        var relationTypes = Enum.GetValues<XmlSchemaAttributeType>();
        foreach (var relType in relationTypes)
        {
            // act 
            var relationTypeResult = IntExtensions.ToXmlSchemaAttributeType((int)relType);
            // assert 
            Assert.Equal(relType, relationTypeResult);
        }
    }

}
