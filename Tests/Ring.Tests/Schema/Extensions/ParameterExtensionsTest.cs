using Ring.Schema.Enums;
using Ring.Schema.Models;
using ResourceHelper = Ring.Util.Helpers.ResourceHelper;
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
            var parameterType = (ParameterType)element;
            var paramTemplate = ResourceHelper.GetParameter(parameterType);
            var defaultValue = paramTemplate.DefaultValue ?? string.Empty;
            result.Add(new Parameter((int)element, _faker.Random.String(), _faker.Random.String(), (ParameterType)element,
                _faker.PickRandom<FieldType>(), defaultValue, defaultValue, schemaId, _faker.PickRandom<EntityType>(),
                true,true));
        }
        // sort by id 
        _parameterCollection = result.OrderBy(o => o.Id).ToArray();
    }

}
