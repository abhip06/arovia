namespace HealthcareStore.Infrastructure.Documents.Receipts;

public sealed class OrderReceiptOptions
{
    public const string SectionName = "OrderReceipt";

    public string BrandName { get; set; } = "Arovia";

    public string Tagline { get; set; } = "Better Health. Made Simple.";

    public string SellerLegalName { get; set; } = string.Empty;

    public string[] SellerAddressLines { get; set; } = [];

    public string SupportPhone { get; set; } = string.Empty;

    public string SupportEmail { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;
}