using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Byok.Errors
{
    internal static class ErrorParser
    {
        /// <summary>
        /// Read the response body and throw the appropriate typed exception.
        /// </summary>
        internal static async Task ThrowForStatusAsync(HttpResponseMessage response)
        {
            var statusCode = (int)response.StatusCode;
            var message = $"BYOK API error ({statusCode})";
            string? code = null;
            string? type = null;
            object? details = null;

            try
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error", out var errorElem))
                {
                    if (errorElem.ValueKind == JsonValueKind.String)
                    {
                        message = errorElem.GetString() ?? message;
                    }
                    else if (errorElem.ValueKind == JsonValueKind.Object)
                    {
                        if (errorElem.TryGetProperty("message", out var msgElem))
                            message = msgElem.GetString() ?? message;
                        if (errorElem.TryGetProperty("code", out var codeElem))
                            code = codeElem.GetString();
                        if (errorElem.TryGetProperty("type", out var typeElem))
                            type = typeElem.GetString();
                        if (errorElem.TryGetProperty("details", out var detailsElem))
                            details = detailsElem.ToString();
                    }
                }
            }
            catch
            {
                // Body wasn't JSON; use status text
                message = response.ReasonPhrase ?? message;
            }

            int? retryAfter = null;
            if (response.Headers.RetryAfter?.Delta != null)
            {
                retryAfter = (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds;
            }

            throw statusCode switch
            {
                401 => new AuthenticationException(message, code, type),
                402 => new PaymentRequiredException(message, code, type),
                403 => new ForbiddenException(message, code, type),
                404 => new NotFoundException(message, code, type),
                422 => new ValidationException(message, code, type, details),
                429 => new RateLimitException(message, code, type, retryAfter),
                502 => new ProviderException(message, code, type),
                _ => new ByokException(message, statusCode, code, type, details),
            };
        }
    }
}
