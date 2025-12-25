using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Base.Common;


public class NullableEnumConverter<T> : JsonConverter<T?> where T : struct, Enum
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            if (Enum.TryParse<T>(stringValue, out var enumValue))
            {
                return enumValue;
            }
            else
            {
                throw new JsonException($"Invalid value {stringValue} for enum {typeof(T)}");
            }
        }
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString());
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}