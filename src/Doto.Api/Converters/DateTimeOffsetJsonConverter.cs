using System.Text.Json;
using System.Text.Json.Serialization;

namespace Doto.Api.Converters;

public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                    throw new JsonException("DateTimeOffset value cannot be null or empty");

                // Use standard parse which handles ISO 8601 automatically
                // RoundtripKind preserves timezone information
                if (DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateTimeOffset))
                {
                    return dateTimeOffset;
                }

                // If standard parse fails, try explicit ISO 8601 formats
                var formats = new[]
                {
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    "yyyy-MM-ddTHH:mm:ss.ffZ",
                    "yyyy-MM-ddTHH:mm:ss.fZ",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    "yyyy-MM-ddTHH:mmZ"
                };

                foreach (var format in formats)
                {
                    if (DateTimeOffset.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                    {
                        return parsed;
                    }
                }

                throw new JsonException($"Unable to convert \"{value}\" to DateTimeOffset. Please use ISO 8601 format (e.g., 2024-11-27T03:15:33.000Z)");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                // Handle Unix timestamp (milliseconds)
                var timestamp = reader.GetInt64();
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            }

            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing DateTimeOffset. Expected String or Number.");
        }
        catch (JsonException)
        {
            throw;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new JsonException($"Date value out of bounds: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error parsing DateTimeOffset: {ex.Message}", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

