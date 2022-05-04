using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trakx.Utils.Serialization.Converters;

public class LongStringConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) return reader.GetInt64();
        var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        return Utf8Parser.TryParse(span, out long number, out var bytesConsumed) && span.Length == bytesConsumed
            ? number
#pragma warning disable S3358 // Ternary operators should not be nested
            : long.TryParse(reader.GetString(), out number)
                ? number
                : reader.GetInt64();
#pragma warning restore S3358 // Ternary operators should not be nested
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class ULongOrStringConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) return reader.GetUInt64();
        var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        return Utf8Parser.TryParse(span, out ulong number, out var bytesConsumed) && span.Length == bytesConsumed
            ? number
#pragma warning disable S3358 // Ternary operators should not be nested
            : ulong.TryParse(reader.GetString(), out number)
                ? number
                : reader.GetUInt64();
#pragma warning restore S3358 // Ternary operators should not be nested
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}