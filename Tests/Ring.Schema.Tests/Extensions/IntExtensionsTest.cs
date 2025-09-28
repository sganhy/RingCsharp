using System.Linq.Expressions;
using Xunit;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Tests.Extensions;

public sealed class IntExtensionsTest 
{
    public IntExtensionsTest() => Expression.Empty();

    [Fact]
    public void ToXmlSchemaAttributeType_AllExistingEnumId_Enum()
    {
        // arrange 
        var relationTypes = Enum.GetValues<SchemaTemplateAttributeType>();
        foreach (var relType in relationTypes)
        {
            // act 
            var relationTypeResult = IntExtensions.ToXmlSchemaAttributeType((int)relType);
            // assert 
            Assert.Equal(relType, relationTypeResult);
        }
    }

}
