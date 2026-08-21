namespace ShippingPlatform.Application.Exceptions;

public sealed class IdempotencyConflictException : Exception
{
    public string IdempotencyKey { get; }

    public IdempotencyConflictException(string idempotencyKey)
        : base($"Idempotency key '{idempotencyKey}' cannot be reused with a different request payload.")
    {
        IdempotencyKey = idempotencyKey;
    }
}
