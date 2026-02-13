using System;

namespace Byok.Errors
{
    /// <summary>Base exception for all BYOK SDK errors.</summary>
    public class ByokException : Exception
    {
        public int StatusCode { get; }
        public string? ErrorCode { get; }
        public string? ErrorType { get; }
        public object? Details { get; }

        public ByokException(string message, int statusCode, string? errorCode = null, string? errorType = null, object? details = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            ErrorType = errorType;
            Details = details;
        }
    }

    /// <summary>401 -- Invalid or missing API key / user credentials.</summary>
    public class AuthenticationException : ByokException
    {
        public AuthenticationException(string message, string? errorCode = null, string? errorType = null)
            : base(message, 401, errorCode, errorType) { }
    }

    /// <summary>402 -- No wallet, insufficient balance, or wallet frozen.</summary>
    public class PaymentRequiredException : ByokException
    {
        public PaymentRequiredException(string message, string? errorCode = null, string? errorType = null)
            : base(message, 402, errorCode, errorType) { }
    }

    /// <summary>403 -- Game not active, tier unavailable, consent not granted.</summary>
    public class ForbiddenException : ByokException
    {
        public ForbiddenException(string message, string? errorCode = null, string? errorType = null)
            : base(message, 403, errorCode, errorType) { }
    }

    /// <summary>404 -- Resource not found.</summary>
    public class NotFoundException : ByokException
    {
        public NotFoundException(string message, string? errorCode = null, string? errorType = null)
            : base(message, 404, errorCode, errorType) { }
    }

    /// <summary>422 -- Validation failed.</summary>
    public class ValidationException : ByokException
    {
        public ValidationException(string message, string? errorCode = null, string? errorType = null, object? details = null)
            : base(message, 422, errorCode, errorType, details) { }
    }

    /// <summary>429 -- Rate limited or quota exceeded.</summary>
    public class RateLimitException : ByokException
    {
        /// <summary>Seconds to wait before retrying (from Retry-After header).</summary>
        public int? RetryAfter { get; }

        public RateLimitException(string message, string? errorCode = null, string? errorType = null, int? retryAfter = null)
            : base(message, 429, errorCode, errorType)
        {
            RetryAfter = retryAfter;
        }
    }

    /// <summary>502 -- Upstream AI provider error.</summary>
    public class ProviderException : ByokException
    {
        public ProviderException(string message, string? errorCode = null, string? errorType = null)
            : base(message, 502, errorCode, errorType) { }
    }
}
