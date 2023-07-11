using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDelta.Util;

public class StringOrEmbedJsonConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        switch (document.RootElement.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var dictionary = new Dictionary<string, object>();
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    dictionary[property.Name] = property.Value;
                }

                return dictionary;
            }
            case JsonValueKind.String:
                return document.RootElement.GetString();
            case JsonValueKind.Undefined:
            case JsonValueKind.Array:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            default:
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;
            case Dictionary<string, object> dictionaryValue:
            {
                writer.WriteStartObject();
                foreach (var entry in dictionaryValue)
                {
                    writer.WritePropertyName(entry.Key);
                    JsonSerializer.Serialize(writer, entry.Value, options);
                }

                writer.WriteEndObject();
                break;
            }
        }
    }
}