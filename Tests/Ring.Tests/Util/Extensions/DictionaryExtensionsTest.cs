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


	[Fact]
	public void GetInt32Value_RandomKey_RandomInt32()
	{
		// arrange 
		Dictionary<string, string> dico = new ();
		var elementCount = 7;
		for (var i = 0; i < elementCount; ++i)
		{
			var newKey = _faker.Random.String(22);
			var newValue = _faker.Random.String(10);
			dico.TryAdd(newKey, newValue);
		}
		var testedKey = _faker.Random.String(22);
		dico[testedKey] = "22";

		// act 
		var result = DictionaryExtensions.GetInt32Value(dico, testedKey);

		// assert
		Assert.Equal(22, result);
	}

	[Fact]
	public void GetInt32Value_InvalidInteger_Null()
	{
		// arrange 
		var dico = new Dictionary<string, string>();
		var elementCount = 12;
		for (var i = 0; i < elementCount; ++i)
		{
			var newKey = _faker.Random.String(22);
			var newValue = _faker.Random.String(10);
			dico.TryAdd(newKey, newValue);
		}
		var testedKey = _faker.Random.String(22);
		dico[testedKey] = "<<22>>";

		// act 
		var result = DictionaryExtensions.GetInt32Value(dico, testedKey);

		// assert
		Assert.Null(result);
	}



	[Fact]
	public void GetInt32Value_UnexistingKey_DefaultValue()
	{
		// arrange 
		var dico = new Dictionary<string, string>();
		dico.TryAdd("1", _faker.Random.String(10));
		dico.TryAdd("2", _faker.Random.String(10));
		dico.TryAdd("3", _faker.Random.String(10));
		dico.TryAdd("4", _faker.Random.String(10));
		var testedKey = "5";

		// act 
		var result = DictionaryExtensions.GetInt32Value(dico, testedKey, 55);

		// assert
		Assert.Equal(55, result);
	}

	[Fact]
	public void GetStringValue_RandomKey_DefaultValue()
	{
		// arrange 
		var expectedValue = _faker.Random.String(10);
		var dico = new Dictionary<string, string>();
		dico.TryAdd("1", _faker.Random.String(10));
		dico.TryAdd("2", _faker.Random.String(10));
		dico.TryAdd("3", _faker.Random.String(10));
		dico.TryAdd("4", expectedValue);

		// act 
		var result = DictionaryExtensions.GetStringValue(dico, "4", "--->");

		// assert
		Assert.Equal(expectedValue, result);
	}


}
