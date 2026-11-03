using HealthcareStore.Domain.Common;

namespace HealthcareStore.Domain.Payments;

public sealed class PaymentEvent : Entity
{
    private PaymentEvent()
    {
    }

    private PaymentEvent(
        Guid paymentId,
        Guid? paymentAttemptId,
        PaymentEventType eventType,
        PaymentEventSource source,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset recordedAtUtc,
        string? providerEventId,
        string? providerOrderId,
        string? providerPaymentId,
        decimal? amount,
        string? currency,
        string? description)
    {
        PaymentId = paymentId;
        PaymentAttemptId = paymentAttemptId;
        EventType = eventType;
        Source = source;

        OccurredAtUtc = occurredAtUtc;
        RecordedAtUtc = recordedAtUtc;

        ProviderEventId = providerEventId;
        ProviderOrderId = providerOrderId;
        ProviderPaymentId = providerPaymentId;

        Amount = amount;
        Currency = currency;
        Description = description;
    }

    public Guid PaymentId { get; private set; }

    public Guid? PaymentAttemptId { get; private set; }

    public PaymentEventType EventType { get; private set; }

    public PaymentEventSource Source { get; private set; }

    public string? ProviderEventId { get; private set; }

    public string? ProviderOrderId { get; private set; }

    public string? ProviderPaymentId { get; private set; }

    public decimal? Amount { get; private set; }

    public string? Currency { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public DateTimeOffset RecordedAtUtc { get; private set; }

    public string? Description { get; private set; }

    public static PaymentEvent Create(
        Guid paymentId,
        Guid? paymentAttemptId,
        PaymentEventType eventType,
        PaymentEventSource source,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset recordedAtUtc,
        string? providerEventId = null,
        string? providerOrderId = null,
        string? providerPaymentId = null,
        decimal? amount = null,
        string? currency = null,
        string? description = null)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Payment ID cannot be empty.",
                nameof(paymentId));
        }

        if (paymentAttemptId == Guid.Empty)
        {
            throw new ArgumentException(
                "Payment attempt ID cannot be empty.",
                nameof(paymentAttemptId));
        }

        PaymentDomainValidation.EnsureDefined(
            eventType,
            nameof(eventType));

        PaymentDomainValidation.EnsureDefined(
            source,
            nameof(source));

        PaymentDomainValidation.EnsureUtc(
            occurredAtUtc,
            nameof(occurredAtUtc));

        PaymentDomainValidation.EnsureUtc(
            recordedAtUtc,
            nameof(recordedAtUtc));

        if (amount.HasValue && amount.Value < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Event amount cannot be negative.");
        }

        var normalizedCurrency = string.IsNullOrWhiteSpace(currency)
            ? null
            : PaymentDomainValidation.NormalizeCurrency(
                currency,
                nameof(currency));

        if (amount.HasValue != (normalizedCurrency is not null))
        {
            throw new ArgumentException(
                "Amount and currency must either both be provided or both be null.");
        }

        return new PaymentEvent(
            paymentId,
            paymentAttemptId,
            eventType,
            source,
            occurredAtUtc,
            recordedAtUtc,
            PaymentDomainValidation.OptionalText(
                providerEventId,
                nameof(providerEventId),
                maxLength: 128),
            PaymentDomainValidation.OptionalText(
                providerOrderId,
                nameof(providerOrderId),
                maxLength: 128),
            PaymentDomainValidation.OptionalText(
                providerPaymentId,
                nameof(providerPaymentId),
                maxLength: 128),
            amount,
            normalizedCurrency,
            PaymentDomainValidation.OptionalText(
                description,
                nameof(description),
                maxLength: 500));
    }
}