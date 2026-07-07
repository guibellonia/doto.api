using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Doto.Api.Middleware;

public class RequestLoggingMiddleware
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    private static readonly string[] SensitiveKeys =
    [
        "authorization", "password", "token", "secret",
        "service_role", "anon_key", "apikey", "api_key"
    ];

    private const string Redacted = "[REDACTED]";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly string? _monitorUrl;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _monitorUrl = configuration["Monitor:Url"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_monitorUrl))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        context.Request.EnableBuffering();
        var requestBody = await ReadRequestBodyAsync(context.Request);

        var originalResponseBody = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        string? error = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            _ = SendToMonitorAsync(context, requestBody, responseBodyText, stopwatch.ElapsedMilliseconds, error);
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
        {
            return string.Empty;
        }

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private async Task SendToMonitorAsync(
        HttpContext context,
        string requestBody,
        string responseBody,
        long durationMs,
        string? error)
    {
        try
        {
            var payload = new
            {
                source = "doto.api",
                direction = "incoming",
                method = context.Request.Method,
                url = $"{context.Request.Path}{context.Request.QueryString}",
                statusCode = context.Response.StatusCode,
                durationMs,
                requestHeaders = RedactHeaders(context.Request.Headers),
                requestBody = ParseBody(requestBody),
                responseHeaders = RedactHeaders(context.Response.Headers),
                responseBody = ParseBody(responseBody),
                error
            };

            using var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            await HttpClient.PostAsync($"{_monitorUrl!.TrimEnd('/')}/events", content);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to send request event to doto.monitor");
        }
    }

    private static Dictionary<string, string> RedactHeaders(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string>();
        foreach (var (key, value) in headers)
        {
            result[key] = SensitiveKeys.Any(s => key.Contains(s, StringComparison.OrdinalIgnoreCase))
                ? Redacted
                : value.ToString();
        }
        return result;
    }

    private static JsonNode? ParseBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(body);
            RedactNode(node);
            return node;
        }
        catch (JsonException)
        {
            return JsonValue.Create(body);
        }
    }

    private static void RedactNode(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var key in obj.Select(kv => kv.Key).ToList())
            {
                if (SensitiveKeys.Any(s => key.Contains(s, StringComparison.OrdinalIgnoreCase)))
                {
                    obj[key] = Redacted;
                }
                else
                {
                    RedactNode(obj[key]);
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                RedactNode(item);
            }
        }
    }
}
