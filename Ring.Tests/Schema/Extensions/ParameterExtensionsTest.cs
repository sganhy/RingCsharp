using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Schema.Extensions;
using Bogus;

namespace Ring.Tests.Schema.Extensions;

public class ParameterExtensionsTest
{
    private readonly Parameter[] _parameterCollection;
    private readonly Faker _faker = new();
    private const int schemaId = 888;

    public ParameterExtensionsTest()
    {
        // create collection of Parameter 
        var result = new List<Parameter>();
        foreach (var element in Enum.GetValues(typeof(ParameterType)))
        {
            result.Add(new Parameter((int)element, _faker.Random.String(), _faker.Random.String(), (ParameterType)element,
                _faker.PickRandom<FieldType>(), ((ParameterType)element).GetDefaultValue() ?? string.Empty, 
                ((ParameterType)element).GetDefaultValue() ?? string.Empty, schemaId, _faker.PickRandom<EntityType>(),
                true,true));
        }
        // sort by id 
        _parameterCollection = result.OrderBy(o => o.Id).ToArray();
    }

}
