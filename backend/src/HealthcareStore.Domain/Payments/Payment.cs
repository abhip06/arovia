using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Payments;

public sealed class Payment : AggregateRoot
{
    private Payment()
    {
    }

    private Payment(
        Guid orderId,
        PaymentProvider provider,
        decimal amount,
        string currency,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        OrderId = orderId;
        Provider = provider;
        Amount = amount;
        Currency = currency;

        Status = PaymentStatus.Pending;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid OrderId { get; private set; }

    public string? ProviderOrderId { get; private set; }

    public PaymentProvider Provider { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = null!;

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? CapturedAtUtc { get; private set; }

    public DateTimeOffset? ExpiredAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public static Payment Create(
        Guid orderId,
        PaymentProvider provider,
        decimal amount,
        string currency,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID cannot be empty.",
                nameof(orderId));
        }

        PaymentDomainValidation.EnsureDefined(
            provider,
            nameof(provider));

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Payment amount must be greater than zero.");
        }

        currency = PaymentDomainValidation.NormalizeCurrency(
            currency,
            nameof(currency));

        PaymentDomainValidation.EnsureUtc(
            createdAtUtc,
            nameof(createdAtUtc));

        PaymentDomainValidation.EnsureUtc(
            expiresAtUtc,
            nameof(expiresAtUtc));

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException(
                "Payment expiry must be later than payment creation.",
                nameof(expiresAtUtc));
        }

        return new Payment(
            orderId,
            provider,
            amount,
            currency,
            createdAtUtc,
            expiresAtUtc);
    }

    public bool SetProviderOrderId(
        string providerOrderId,
        DateTimeOffset recordedAtUtc)
    {
        ValidateRecordedAt(recordedAtUtc);

        providerOrderId = PaymentDomainValidation.RequiredText(
            providerOrderId,
            nameof(providerOrderId),
            maxLength: 128);

        if (ProviderOrderId is not null)
        {
            if (ProviderOrderId != providerOrderId)
            {
                throw new InvalidOperationException(
                    "The payment already has a different provider order ID.");
            }

            return false;
        }

        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException(
                "A provider order can only be attached to a pending payment.");
        }

        ProviderOrderId = providerOrderId;
        Touch(recordedAtUtc);

        return true;
    }

    public bool MarkCaptured(
        DateTimeOffset capturedAtUtc,
        DateTimeOffset recordedAtUtc)
    {
        PaymentDomainValidation.EnsureNotBefore(
            capturedAtUtc,
            CreatedAtUtc,
            nameof(capturedAtUtc));

        ValidateRecordedAt(recordedAtUtc);

        if (Status == PaymentStatus.Captured)
        {
            return false;
        }

        Status = PaymentStatus.Captured;
        CapturedAtUtc = capturedAtUtc;

        Touch(recordedAtUtc);

        return true;
    }

    public bool Expire(DateTimeOffset nowUtc)
    {
        ValidateRecordedAt(nowUtc);

        if (Status != PaymentStatus.Pending)
        {
            return false;
        }

        if (nowUtc < ExpiresAtUtc)
        {
            throw new InvalidOperationException(
                "A payment cannot expire before its expiry time.");
        }

        Status = PaymentStatus.Expired;
        ExpiredAtUtc = nowUtc;

        Touch(nowUtc);

        return true;
    }

    public bool Cancel(DateTimeOffset nowUtc)
    {
        ValidateRecordedAt(nowUtc);

        if (Status != PaymentStatus.Pending)
        {
            return false;
        }

        Status = PaymentStatus.Cancelled;
        CancelledAtUtc = nowUtc;

        Touch(nowUtc);

        return true;
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