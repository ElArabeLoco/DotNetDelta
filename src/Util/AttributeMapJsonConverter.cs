using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDelta.Util;

public class AttributeMapJsonConverter : JsonConverter<AttributeMap>
{
    public override AttributeMap? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var attributesElement = JsonDocument.ParseValue(ref reader);

        if (attributesElement.RootElement.ValueKind == JsonValueKind.Object)
        {
            var attributeMap = new AttributeMap();
            foreach (var property in attributesElement.RootElement.EnumerateObject())
            {
                object? value;
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        value = property.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        value = property.Value.GetInt64();
                        break;
                    case JsonValueKind.True:
                        value = true;
                        break;
                    case JsonValueKind.False:
                        value = false;
                        break;
                    case JsonValueKind.Object:
                        value = property.Value;
                        break;
                    case JsonValueKind.Array:
                        value = property.Value;
                        break;
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Null:
                    default:
                        value = null;
                        break;
                }

                attributeMap[property.Name] = value;
            }

            return attributeMap;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, AttributeMap value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var entry in value)
        {
            writer.WritePropertyName(entry.Key);
            JsonSerializer.Serialize(writer, entry.Value, options);
        }
        writer.WriteEndObject();
    }
}
