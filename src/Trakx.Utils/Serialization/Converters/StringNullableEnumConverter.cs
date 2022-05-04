using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable IDE0046 // Convert to conditional expression

namespace Trakx.Utils.Serialization.Converters;

/// <summary>
/// Thanks to https://github.com/dotnet/corefx/issues/41307#issuecomment-562845257
/// </summary>
/// <typeparam name="T"></typeparam>
public class StringNullableEnumConverter<T> : JsonConverter<T>
{
    private readonly Type? _underlyingType;

    public StringNullableEnumConverter()
    {
        _underlyingType = Nullable.GetUnderlyingType(typeof(T));
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(T).IsAssignableFrom(typeToConvert);
    }

    public override T? Read(ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value)) return default;
        object? result = null;
        if (_underlyingType != null 
            && !Enum.TryParse(_underlyingType, value, ignoreCase: false, out result) 
            && !Enum.TryParse(_underlyingType, value, ignoreCase: true, out result))
        {
            throw new JsonException($"Unable to convert \"{value}\" to Enum \"{_underlyingType}\".");
        }
        return (T?) result;
    }

    public override void Write(Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}