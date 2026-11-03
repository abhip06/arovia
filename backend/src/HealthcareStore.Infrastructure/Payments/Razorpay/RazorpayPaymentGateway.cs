using HealthcareStore.Application.Abstractions.Payments;
using HealthcareStore.Application.Payments.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthcareStore.Infrastructure.Payments.Razorpay;

public sealed class RazorpayPaymentGateway : IPaymentGateway
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly RazorpayOptions _options;

    public RazorpayPaymentGateway(
        HttpClient httpClient,
        IOptions<RazorpayOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        var credentials = Encoding.UTF8.GetBytes(
            $"{_options.KeyId}:{_options.KeySecret}");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(credentials));
    }

    public string PublicKeyId => _options.KeyId;

    public async Task<GatewayOrderResult> CreateOrderAsync(
        CreateGatewayOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.AmountSubunits < 100 &&
            string.Equals(
                request.Currency,
                "INR",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Razorpay requires an INR payment of at least 100 paise.");
        }

        if (request.AmountSubunits <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "The gateway amount must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.Currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Receipt);

        if (request.Receipt.Length > 40)
        {
            throw new ArgumentException(
                "Razorpay receipts cannot exceed 40 characters.",
                nameof(request));
        }

        using var response = await _httpClient.PostAsJsonAsync(
            "orders",
            new CreateRazorpayOrderRequest(
                request.AmountSubunits,
                request.Currency.ToUpperInvariant(),
                request.Receipt,
                PartialPayment: false),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<RazorpayOrderDto>(
                JsonOptions,
                cancellationToken);

        return MapOrder(
            result ?? throw new InvalidDataException(
                "Razorpay returned an empty order response."));
    }

    public async Task<GatewayOrderResult?> FindOrderByReceiptAsync(
        string receipt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(receipt);

        var path =
            $"orders?receipt={Uri.EscapeDataString(receipt)}&count=100";

        using var response = await _httpClient.GetAsync(
            path,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<RazorpayOrderCollectionDto>(
                JsonOptions,
                cancellationToken);

        var matches = (result?.Items ?? [])
            .Where(order => string.Equals(
                order.Receipt,
                receipt,
                StringComparison.Ordinal))
            .ToArray();

        return matches.Length switch
        {
            0 => null,

            1 => MapOrder(matches[0]),

            _ => throw new InvalidOperationException(
                "Razorpay returned multiple orders for the same receipt.")
        };
    }

    public async Task<GatewayPaymentResult> GetPaymentAsync(
        string providerPaymentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerPaymentId);

        using var response = await _httpClient.GetAsync(
            $"payments/{Uri.EscapeDataString(providerPaymentId)}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<RazorpayPaymentDto>(
                JsonOptions,
                cancellationToken);

        var payment = result
            ?? throw new InvalidDataException(
                "Razorpay returned an empty payment response.");

        return new GatewayPaymentResult(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.Method,
            payment.Captured,
            DateTimeOffset.FromUnixTimeSeconds(payment.CreatedAt));
    }

    public bool VerifyCheckoutSignature(
        string trustedProviderOrderId,
        string providerPaymentId,
        string signature)
    {
        if (string.IsNullOrWhiteSpace(trustedProviderOrderId) ||
            string.IsNullOrWhiteSpace(providerPaymentId))
        {
            return false;
        }

        var payload = $"{trustedProviderOrderId}|{providerPaymentId}";

        return VerifyHmacSha256(
            _options.KeySecret,
            payload,
            signature);
    }

    public bool VerifyWebhookSignature(
        string rawBody,
        string signature)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            return false;
        }

        return VerifyHmacSha256(
            _options.WebhookSecret,
            rawBody,
            signature);
    }

    private static bool VerifyHmacSha256(
        string secret,
        string payload,
        string signature)
    {
        if (string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        byte[] suppliedSignature;

        try
        {
            suppliedSignature = Convert.FromHexString(signature);
        }
        catch (FormatException)
        {
            return false;
        }

        var expectedSignature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(payload));

        return suppliedSignature.Length == expectedSignature.Length &&
            CryptographicOperations.FixedTimeEquals(
                suppliedSignature,
                expectedSignature);
    }

    private static GatewayOrderResult MapOrder(
        RazorpayOrderDto order)
    {
        return new GatewayOrderResult(
            order.Id,
            order.Amount,
            order.Currency,
            order.Receipt,
            order.Status,
            DateTimeOffset.FromUnixTimeSeconds(order.CreatedAt));
    }

    private sealed record CreateRazorpayOrderRequest(
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("receipt")] string Receipt,
        [property: JsonPropertyName("partial_payment")] bool PartialPayment);

    private sealed record RazorpayOrderDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("receipt")] string? Receipt,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("created_at")] long CreatedAt);

    private sealed record RazorpayOrderCollectionDto(
        [property: JsonPropertyName("items")]
        List<RazorpayOrderDto>? Items);

    private sealed record RazorpayPaymentDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("order_id")] string? OrderId,
        [property: JsonPropertyName("amount")] long Amount,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("method")] string Method,
        [property: JsonPropertyName("captured")] bool Captured,
        [property: JsonPropertyName("created_at")] long CreatedAt);
}