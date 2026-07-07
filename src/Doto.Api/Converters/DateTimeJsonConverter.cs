using System.Text.Json;
using System.Text.Json.Serialization;

namespace Doto.Api.Converters;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                    throw new JsonException("DateTime value cannot be null or empty");

                // Use standard parse which handles ISO 8601 automatically
                if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    // Ensure UTC timezone
                    if (dateTime.Kind == DateTimeKind.Unspecified)
                    {
                        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    }
                    return dateTime.ToUniversalTime();
                }

                // If standard parse fails, try explicit ISO 8601 formats
                var formats = new[]
                {
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    "yyyy-MM-ddTHH:mm:ss.ffZ",
                    "yyyy-MM-ddTHH:mm:ss.fZ",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    "yyyy-MM-ddTHH:mmZ",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-ddTHH:mm"
                };

                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                    {
                        if (parsed.Kind == DateTimeKind.Unspecified)
                        {
                            parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
                        }
                        return parsed.ToUniversalTime();
                    }
                }

                throw new JsonException($"Unable to convert \"{value}\" to DateTime. Please use ISO 8601 format (e.g., 2024-11-27T03:15:33.000Z)");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                // Handle Unix timestamp (milliseconds)
                var timestamp = reader.GetInt64();
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            }

            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing DateTime. Expected String or Number.");
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
            throw new JsonException($"Error parsing DateTime: {ex.Message}", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcValue = value.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc) 
            : value.ToUniversalTime();
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

