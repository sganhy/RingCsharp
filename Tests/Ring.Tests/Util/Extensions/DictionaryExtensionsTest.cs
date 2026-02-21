using Ring.Util.Extensions;
using System.Linq.Expressions;

namespace Ring.Tests.Util.Extensions;

public class DictionaryExtensionsTest : BaseTest
{
    public DictionaryExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    public void ClearValues_RandomKeyValues_ValuesCleared()
    {
        // arrange 
        var dico = new Dictionary<string, string?>();
        var elementCount = 9;
        for (var i = 0; i < elementCount; ++i)
        { 
            var newKey  = _faker.Random.String(22);
            var newValue = _faker.Random.String(10);
            if (!dico.ContainsKey(newKey)) dico.Add(newKey, newValue);
        }
        var emptyMarker = _faker.Random.String(10); 

        // act 
        dico.ClearValues(emptyMarker);

        // assert
        foreach (var pair in dico)  Assert.Equal(emptyMarker, pair.Value);
    }
}
