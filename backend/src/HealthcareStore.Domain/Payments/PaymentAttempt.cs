using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Payments;

public sealed class PaymentAttempt : Entity
{
    private PaymentAttempt()
    {
    }

    private PaymentAttempt(
        Guid paymentId,
        string idempotencyKey,
        string requestHash,
        DateTimeOffset createdAtUtc)
    {
        PaymentId = paymentId;
        IdempotencyKey = idempotencyKey;
        RequestHash = requestHash;

        Status = PaymentAttemptStatus.Initiated;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid PaymentId { get; private set; }

    public string IdempotencyKey { get; private set; } = null!;

    public string RequestHash { get; private set; } = null!;

    public string? ProviderPaymentId { get; private set; }

    public PaymentMethod? Method { get; private set; }

    public PaymentAttemptStatus Status { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureDescription { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? AuthorizedAtUtc { get; private set; }

    public DateTimeOffset? CapturedAtUtc { get; private set; }

    public DateTimeOffset? FailedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset? ExpiredAtUtc { get; private set; }

    public static PaymentAttempt Create(
        Guid paymentId,
        string idempotencyKey,
        string requestHash,
        DateTimeOffset createdAtUtc)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Payment ID cannot be empty.",
                nameof(paymentId));
        }

        idempotencyKey = PaymentDomainValidation.RequiredText(
            idempotencyKey,
            nameof(idempotencyKey),
            maxLength: 128);

        requestHash = PaymentDomainValidation.NormalizeSha256Hash(
            requestHash,
            nameof(requestHash));

        PaymentDomainValidation.EnsureUtc(
            createdAtUtc,
            nameof(createdAtUtc));

        return new PaymentAttempt(
            paymentId,
            idempotencyKey,
            requestHash,
            createdAtUtc);
    }

    public bool MarkAuthorized(
        string providerPaymentId,
        PaymentMethod method,
        DateTimeOffset authorizedAtUtc,
        DateTimeOffset recordedAtUtc)
    {
        ValidateOccurredAt(authorizedAtUtc);
        ValidateRecordedAt(recordedAtUtc);

        PaymentDomainValidation.EnsureDefined(
            method,
            nameof(method));

        if (Status is PaymentAttemptStatus.Captured
            or PaymentAttemptStatus.Cancelled
            or PaymentAttemptStatus.Expired)
        {
            return false;
        }

        ProviderPaymentId = PaymentDomainValidation.RequiredText(
            providerPaymentId,
            nameof(providerPaymentId),
            maxLength: 128);

        Method = method;
        Status = PaymentAttemptStatus.Authorized;
        AuthorizedAtUtc = authorizedAtUtc;

        FailureCode = null;
        FailureDescription = null;

        Touch(recordedAtUtc);

        return true;
    }

    public bool MarkFailed(
        string? providerPaymentId,
        PaymentMethod? method,
        string? failureCode,
        string? failureDescription,
        DateTimeOffset failedAtUtc,
        DateTimeOffset recordedAtUtc)
    {
        ValidateOccurredAt(failedAtUtc);
        ValidateRecordedAt(recordedAtUtc);

        if (Status == PaymentAttemptStatus.Captured)
        {
            return false;
        }

        if (method.HasValue)
        {
            PaymentDomainValidation.EnsureDefined(
                method.Value,
                nameof(method));
        }

        ProviderPaymentId = providerPaymentId is null
            ? ProviderPaymentId
            : PaymentDomainValidation.RequiredText(
                providerPaymentId,
                nameof(providerPaymentId),
                maxLength: 128);

        Method = method ?? Method;

        FailureCode = PaymentDomainValidation.OptionalText(
            failureCode,
            nameof(failureCode),
            maxLength: 100);

        FailureDescription = PaymentDomainValidation.OptionalText(
            failureDescription,
            nameof(failureDescription),
            maxLength: 500);

        Status = PaymentAttemptStatus.Failed;
        FailedAtUtc = failedAtUtc;

        Touch(recordedAtUtc);

        return true;
    }

    public bool MarkCaptured(
        string providerPaymentId,
        PaymentMethod method,
        DateTimeOffset capturedAtUtc,
        DateTimeOffset recordedAtUtc)
    {
        ValidateOccurredAt(capturedAtUtc);
        ValidateRecordedAt(recordedAtUtc);

        PaymentDomainValidation.EnsureDefined(
            method,
            nameof(method));

        if (Status == PaymentAttemptStatus.Captured)
        {
            return false;
        }

        ProviderPaymentId = PaymentDomainValidation.RequiredText(
            providerPaymentId,
            nameof(providerPaymentId),
            maxLength: 128);

        Method = method;
        Status = PaymentAttemptStatus.Captured;
        CapturedAtUtc = capturedAtUtc;

        FailureCode = null;
        FailureDescription = null;

        Touch(recordedAtUtc);

        return true;
    }

    public bool Cancel(DateTimeOffset nowUtc)
    {
        ValidateRecordedAt(nowUtc);

        if (Status is PaymentAttemptStatus.Captured
            or PaymentAttemptStatus.Failed
            or PaymentAttemptStatus.Cancelled
            or PaymentAttemptStatus.Expired)
        {
            return false;
        }

        Status = PaymentAttemptStatus.Cancelled;
        CancelledAtUtc = nowUtc;

        Touch(nowUtc);

        return true;
    }

    public bool Expire(DateTimeOffset nowUtc)
    {
        ValidateRecordedAt(nowUtc);

        if (Status is not (PaymentAttemptStatus.Initiated
            or PaymentAttemptStatus.Pending
            or PaymentAttemptStatus.Authorized))
        {
            return false;
        }

        Status = PaymentAttemptStatus.Expired;
        ExpiredAtUtc = nowUtc;

        Touch(nowUtc);

        return true;
    }

    public bool MarkPending(DateTimeOffset recordedAtUtc)
    {
        ValidateRecordedAt(recordedAtUtc);

        if (Status == PaymentAttemptStatus.Pending)
        {
            return false;
        }

        if (Status != PaymentAttemptStatus.Initiated)
        {
            throw new InvalidOperationException(
                "Only an initiated payment attempt can become pending.");
        }

        Status = PaymentAttemptStatus.Pending;
        Touch(recordedAtUtc);

        return true;
    }

    public bool TryAcquireGatewayOrderInitialization(
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration)
    {
        PaymentDomainValidation.EnsureUtc(nowUtc, nameof(nowUtc));

        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(leaseDuration));
        }

        if (Status != PaymentAttemptStatus.Initiated)
        {
            return false;
        }

        if (nowUtc - UpdatedAtUtc < leaseDuration)
        {
            return false;
        }

        Touch(nowUtc);

        return true;
    }

    private void ValidateOccurredAt(DateTimeOffset occurredAtUtc)
    {
        PaymentDomainValidation.EnsureNotBefore(
            occurredAtUtc,
            CreatedAtUtc,
            nameof(occurredAtUtc));
    }

    private void ValidateRecordedAt(DateTimeOffset recordedAtUtc)
    {
        PaymentDomainValidation.EnsureNotBefore(
            recordedAtUtc,
            CreatedAtUtc,
            nameof(recordedAtUtc));
    }

    private void Touch(DateTimeOffset recordedAtUtc)
    {
        if (recordedAtUtc > UpdatedAtUtc)
        {
            UpdatedAtUtc = recordedAtUtc;
        }
    }
}