using System.Text.Json;
using System.Text.Json.Nodes;

namespace IAX.IXMcp.Execution;

public sealed class ToolArgumentValidator
{
    public string? Validate(JsonElement value, JsonObject schema, string location = "arguments")
    {
        var expectedType = schema["type"]?.GetValue<string>();
        if (!MatchesType(value, expectedType))
        {
            return $"{location} must be {expectedType}.";
        }

        if (expectedType == "object")
        {
            var properties = schema["properties"] as JsonObject ?? new JsonObject();
            var required = (schema["required"] as JsonArray)?
                .Select(node => node!.GetValue<string>())
                .ToHashSet(StringComparer.Ordinal) ?? [];

            foreach (var requiredName in required)
            {
                if (!value.TryGetProperty(requiredName, out _))
                {
                    return $"{location}.{requiredName} is required.";
                }
            }

            foreach (var property in value.EnumerateObject())
            {
                if (!properties.TryGetPropertyValue(property.Name, out var propertySchema)
                    || propertySchema is not JsonObject propertyObject)
                {
                    return $"{location}.{property.Name} is not allowed.";
                }

                var error = Validate(property.Value, propertyObject, $"{location}.{property.Name}");
                if (error is not null)
                {
                    return error;
                }
            }
        }

        if (expectedType is "integer" or "number" && value.TryGetDecimal(out var number))
        {
            if (ReadDecimal(schema["minimum"]) is { } minimum && number < minimum)
            {
                return $"{location} must be at least {minimum}.";
            }

            if (ReadDecimal(schema["maximum"]) is { } maximum && number > maximum)
            {
                return $"{location} must be at most {maximum}.";
            }
        }

        return null;
    }

    private static bool MatchesType(JsonElement value, string? expectedType) => expectedType switch
    {
        "object" => value.ValueKind == JsonValueKind.Object,
        "array" => value.ValueKind == JsonValueKind.Array,
        "string" => value.ValueKind == JsonValueKind.String,
        "integer" => value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),
        "number" => value.ValueKind == JsonValueKind.Number,
        "boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
        null => true,
        _ => false
    };

    private static decimal? ReadDecimal(JsonNode? node)
    {
        if (node is not JsonValue value)
        {
            return null;
        }

        if (value.TryGetValue<decimal>(out var decimalValue)) return decimalValue;
        if (value.TryGetValue<long>(out var longValue)) return longValue;
        if (value.TryGetValue<int>(out var intValue)) return intValue;
        if (value.TryGetValue<double>(out var doubleValue)) return (decimal)doubleValue;
        return null;
    }
}
