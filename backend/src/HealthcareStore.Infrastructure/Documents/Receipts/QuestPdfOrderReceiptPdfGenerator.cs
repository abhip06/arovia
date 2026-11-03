using System.Globalization;
using HealthcareStore.Application.Abstractions.Documents;
using HealthcareStore.Application.Orders.Receipts;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HealthcareStore.Infrastructure.Documents.Receipts;

/// <summary>
/// Arovia customer order/payment receipt. The output stream is owned by the caller.
/// This is an order receipt, not a statutory tax invoice: it only renders tax data
/// that the current order snapshot actually persists.
/// </summary>
public sealed class QuestPdfOrderReceiptPdfGenerator(
    IOptions<OrderReceiptOptions> options) : IOrderReceiptPdfGenerator
{
    private const string Green = "#16824B";
    private const string Blue = "#2563EB";
    private const string Dark = "#1F3027";
    private const string Muted = "#66776D";
    private const string Line = "#DDE7E0";
    private const string SoftGreen = "#F0F8F2";
    private const string SoftBlue = "#F1F6FF";
    private const string SoftGray = "#F8FAF8";

    private static readonly CultureInfo IndianCulture = CultureInfo.GetCultureInfo("en-IN");

    private readonly OrderReceiptOptions _options = options.Value;

    // This lightweight vector mark matches the leaf motif used in the Arovia preview.
    // Replace the SVG with the approved brand logo if the final logo asset differs.
    private const string AroviaMarkSvg = """
        <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64">
          <rect x="1" y="1" width="62" height="62" rx="18" fill="#16824B"/>
          <path d="M28.8 47.5C17.4 43.7 13.8 33.4 17 22.2c10.6 1 18 8.1 18 18-1.9-8.8 5.5-16.6 15.8-17.3 1.2 13.1-5.9 22.2-22 22.6Z" fill="#FFFFFF"/>
          <path d="M15.5 49.5c8.5-9.6 16.1-15.4 28.2-22.4" fill="none" stroke="#D9F4E2" stroke-width="3" stroke-linecap="round"/>
        </svg>
        """;

    public void Generate(OrderReceiptModel receipt, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(destination);

        if (!destination.CanWrite)
        {
            throw new ArgumentException(
                "The destination stream must be writable.",
                nameof(destination));
        }

        if (receipt.Items.Count == 0)
        {
            throw new ArgumentException(
                "A receipt must contain at least one order item.",
                nameof(receipt));
        }

        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(32);
                page.MarginVertical(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor(Dark));

                page.Header().Element(ComposeHeader);
                page.Content().PaddingTop(18).Element(container => ComposeContent(container, receipt));
                page.Footer().PaddingTop(12).Element(ComposeFooter);
            });
        }).GeneratePdf(destination);
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.Spacing(11);

                row.ConstantItem(42)
                    .Height(42)
                    .Svg(AroviaMarkSvg)
                    .FitArea();

                row.RelativeItem().AlignMiddle().Column(brand =>
                {
                    brand.Item()
                        .Text(_options.BrandName)
                        .FontSize(22)
                        .Bold()
                        .FontColor(Green);

                    brand.Item()
                        .PaddingTop(2)
                        .Text(_options.Tagline)
                        .FontSize(8)
                        .FontColor(Muted);
                });

                row.ConstantItem(190).AlignRight().AlignMiddle().Column(title =>
                {
                    title.Item()
                        .AlignRight()
                        .Text("ORDER RECEIPT")
                        .FontSize(15)
                        .Bold()
                        .FontColor(Dark);

                    title.Item()
                        .PaddingTop(5)
                        .AlignRight()
                        .Text("Payment confirmation")
                        .FontSize(8)
                        .FontColor(Green);
                });
            });

            column.Item()
                .PaddingTop(15)
                .LineHorizontal(1.5f)
                .LineColor(Green);
        });
    }

    private void ComposeContent(IContainer container, OrderReceiptModel receipt)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item().Row(row =>
            {
                row.Spacing(12);
                row.RelativeItem().Element(c => ComposeSellerCard(c));
                row.RelativeItem().Element(c => ComposeCustomerCard(c, receipt.ShippingAddress));
            });

            column.Item().Element(c => ComposeOrderMetadata(c, receipt));

            column.Item().Text("ITEMS PURCHASED")
                .FontSize(9)
                .Bold()
                .FontColor(Dark);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(22);
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(29);
                    columns.ConstantColumn(48);
                    columns.ConstantColumn(48);
                    columns.ConstantColumn(47);
                    columns.ConstantColumn(46);
                    columns.ConstantColumn(62);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell()).Text("#");
                    HeaderCell(header.Cell()).Text("PRODUCT / SKU");
                    HeaderCell(header.Cell()).AlignRight().Text("QTY");
                    HeaderCell(header.Cell()).AlignRight().Text("MRP");
                    HeaderCell(header.Cell()).AlignRight().Text("RATE");
                    HeaderCell(header.Cell()).AlignRight().Text("DISCOUNT");
                    HeaderCell(header.Cell()).AlignRight().Text("TAX");
                    HeaderCell(header.Cell()).AlignRight().Text("AMOUNT");
                });

                for (var index = 0; index < receipt.Items.Count; index++)
                {
                    var item = receipt.Items[index];

                    BodyCell(table.Cell()).Text((index + 1).ToString(CultureInfo.InvariantCulture));

                    BodyCell(table.Cell()).Column(product =>
                    {
                        product.Item()
                            .Text(item.ProductName)
                            .FontSize(8)
                            .SemiBold();

                        var package = $"{item.PackageQuantity.ToString("0.###", IndianCulture)} {item.PackageUnit}";
                        product.Item()
                            .PaddingTop(2)
                            .Text($"{item.Sku} · {package}")
                            .FontSize(7)
                            .FontColor(Muted);
                    });

                    BodyCell(table.Cell()).AlignRight().Text(item.Quantity.ToString(CultureInfo.InvariantCulture));
                    BodyCell(table.Cell()).AlignRight().Text(FormatMoney(receipt.Currency, item.UnitMrp));
                    BodyCell(table.Cell()).AlignRight().Text(FormatMoney(receipt.Currency, item.UnitSellingPrice));
                    BodyCell(table.Cell()).AlignRight().Text(FormatMoney(receipt.Currency, item.LineDiscountAmount));
                    BodyCell(table.Cell()).AlignRight().Text(FormatMoney(receipt.Currency, item.LineTaxAmount));
                    BodyCell(table.Cell()).AlignRight().Text(FormatMoney(receipt.Currency, item.LineTotal)).SemiBold();
                }
            });

            column.Item().Row(row =>
            {
                row.Spacing(14);

                row.RelativeItem().Column(note =>
                {
                    note.Item()
                        .Background(SoftGreen)
                        .BorderLeft(3)
                        .BorderColor(Green)
                        .Padding(11)
                        .Column(message =>
                        {
                            message.Item().Text("Thank you for choosing Arovia.")
                                .FontSize(9)
                                .SemiBold()
                                .FontColor(Green);

                            message.Item().PaddingTop(4)
                                .Text("We hope your healthcare essentials support your everyday wellbeing.")
                                .FontSize(8)
                                .FontColor(Muted);
                        });

                    if (string.Equals(receipt.Currency, "INR", StringComparison.OrdinalIgnoreCase))
                    {
                        note.Item().PaddingTop(10)
                            .Text($"Amount in words: {AmountInWords(receipt.GrandTotal)}")
                            .FontSize(8)
                            .FontColor(Dark);
                    }
                });

                row.ConstantItem(245).Element(c => ComposeTotals(c, receipt));
            });

            column.Item()
                .BorderTop(1)
                .BorderColor(Line)
                .PaddingTop(8)
                .Text("This document is an order/payment receipt, not a statutory tax invoice. Tax registration and item-level tax classifications are not shown because they are not part of the current order snapshot.")
                .FontSize(7)
                .FontColor(Muted);
        });
    }

    private void ComposeSellerCard(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Line)
            .Background(SoftGray)
            .Padding(12)
            .Column(column =>
            {
                column.Item().Text("SOLD BY")
                    .FontSize(7)
                    .Bold()
                    .FontColor(Green);

                column.Item().PaddingTop(6)
                    .Text(_options.SellerLegalName)
                    .FontSize(10)
                    .SemiBold();

                foreach (var addressLine in _options.SellerAddressLines.Where(static line => !string.IsNullOrWhiteSpace(line)))
                {
                    column.Item().PaddingTop(2)
                        .Text(addressLine.Trim())
                        .FontSize(8)
                        .FontColor(Muted);
                }

                if (!string.IsNullOrWhiteSpace(_options.SupportPhone))
                {
                    column.Item().PaddingTop(5)
                        .Text($"Phone: {_options.SupportPhone.Trim()}")
                        .FontSize(8)
                        .FontColor(Muted);
                }

                if (!string.IsNullOrWhiteSpace(_options.SupportEmail))
                {
                    column.Item().PaddingTop(2)
                        .Text($"Email: {_options.SupportEmail.Trim()}")
                        .FontSize(8)
                        .FontColor(Muted);
                }

                if (!string.IsNullOrWhiteSpace(_options.Website))
                {
                    column.Item().PaddingTop(2)
                        .Text(_options.Website.Trim())
                        .FontSize(8)
                        .FontColor(Blue);
                }
            });
    }

    private static void ComposeCustomerCard(
        IContainer container,
        OrderReceiptAddressModel address)
    {
        container
            .Border(1)
            .BorderColor(Line)
            .Padding(12)
            .Column(column =>
            {
                column.Item().Text("DELIVERED TO")
                    .FontSize(7)
                    .Bold()
                    .FontColor(Blue);

                column.Item().PaddingTop(6)
                    .Text(address.RecipientName)
                    .FontSize(10)
                    .SemiBold();

                column.Item().PaddingTop(3).Text(address.AddressLine1).FontSize(8).FontColor(Muted);

                if (!string.IsNullOrWhiteSpace(address.AddressLine2))
                {
                    column.Item().PaddingTop(2).Text(address.AddressLine2).FontSize(8).FontColor(Muted);
                }

                column.Item().PaddingTop(2)
                    .Text($"{address.City}, {address.State} {address.PostalCode}")
                    .FontSize(8)
                    .FontColor(Muted);

                column.Item().PaddingTop(2).Text(address.Country).FontSize(8).FontColor(Muted);
                column.Item().PaddingTop(5).Text($"Phone: {address.RecipientPhone}").FontSize(8).FontColor(Muted);
            });
    }

    private static void ComposeOrderMetadata(IContainer container, OrderReceiptModel receipt)
    {
        container
            .Border(1)
            .BorderColor(Line)
            .PaddingVertical(10)
            .PaddingHorizontal(12)
            .Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("ORDER NUMBER").FontSize(7).Bold().FontColor(Muted);
                    column.Item().PaddingTop(4).Text(receipt.OrderNumber).FontSize(9).SemiBold();
                });

                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("ORDER DATE").FontSize(7).Bold().FontColor(Muted);
                    column.Item().PaddingTop(4).Text(FormatIndianDate(receipt.OrderPlacedAtUtc)).FontSize(8).SemiBold();
                });

                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("PAYMENT CONFIRMED").FontSize(7).Bold().FontColor(Muted);
                    column.Item().PaddingTop(4).Text(FormatIndianDate(receipt.PaymentConfirmedAtUtc)).FontSize(8).SemiBold();
                    column.Item().PaddingTop(2).Text(receipt.PaymentProvider).FontSize(7).FontColor(Green);
                });
            });
    }

    private static void ComposeTotals(IContainer container, OrderReceiptModel receipt)
    {
        container
            .Border(1)
            .BorderColor(Line)
            .Padding(12)
            .Column(column =>
            {
                column.Item().Text("PRICE SUMMARY")
                    .FontSize(8)
                    .Bold()
                    .FontColor(Dark);

                column.Item().PaddingTop(10).Element(c => SummaryLine(c, "Subtotal", FormatMoney(receipt.Currency, receipt.Subtotal)));
                column.Item().PaddingTop(7).Element(c => SummaryLine(c, "Discount", $"−{FormatMoney(receipt.Currency, receipt.DiscountTotal)}"));
                column.Item().PaddingTop(7).Element(c => SummaryLine(c, "Tax", FormatMoney(receipt.Currency, receipt.TaxTotal)));
                column.Item().PaddingTop(7).Element(c => SummaryLine(c, "Delivery", receipt.ShippingFee == 0 ? "Free" : FormatMoney(receipt.Currency, receipt.ShippingFee)));

                column.Item()
                    .PaddingTop(10)
                    .Background(Green)
                    .Padding(9)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("GRAND TOTAL")
                            .FontSize(8)
                            .Bold()
                            .FontColor(Colors.White);

                        row.ConstantItem(112).AlignRight().Text(FormatMoney(receipt.Currency, receipt.GrandTotal))
                            .FontSize(11)
                            .Bold()
                            .FontColor(Colors.White);
                    });
            });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Line);

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text($"{_options.BrandName} · {_options.Tagline}")
                    .FontSize(7)
                    .FontColor(Muted);

                row.ConstantItem(150).AlignRight().Text("Keep this receipt for your records")
                    .FontSize(7)
                    .FontColor(Muted);
            });
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Green).PaddingVertical(7).PaddingHorizontal(4);

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(0.6f).BorderColor(Line).PaddingVertical(7).PaddingHorizontal(4);

    private static void SummaryLine(IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(label).FontSize(8).FontColor(Muted);
            row.ConstantItem(112).AlignRight().Text(value).FontSize(8).SemiBold().FontColor(Dark);
        });
    }

    private static string FormatMoney(string currency, decimal amount)
    {
        var formatted = amount.ToString("N2", IndianCulture);

        return string.Equals(currency, "INR", StringComparison.OrdinalIgnoreCase)
            ? $"₹{formatted}"
            : $"{currency.ToUpperInvariant()} {amount.ToString("N2", CultureInfo.InvariantCulture)}";
    }

    private static string FormatIndianDate(DateTimeOffset value) =>
        value.ToOffset(TimeSpan.FromMinutes(330))
            .ToString("dd MMM yyyy, hh:mm tt 'IST'", CultureInfo.InvariantCulture);

    private static string AmountInWords(decimal amount)
    {
        var rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        var rupees = decimal.ToInt64(decimal.Truncate(rounded));
        var paise = decimal.ToInt32((rounded - rupees) * 100m);

        if (paise == 100)
        {
            rupees++;
            paise = 0;
        }

        var words = $"Rupees {IntegerToIndianWords(rupees)}";

        if (paise > 0)
        {
            words += $" and {IntegerToIndianWords(paise)} Paise";
        }

        return words + " Only";
    }

    private static string IntegerToIndianWords(long number)
    {
        if (number == 0)
        {
            return "Zero";
        }

        if (number < 0)
        {
            return "Minus " + IntegerToIndianWords(Math.Abs(number));
        }

        var parts = new List<string>();

        if (number >= 10_000_000)
        {
            parts.Add($"{IntegerToIndianWords(number / 10_000_000)} Crore");
            number %= 10_000_000;
        }

        if (number >= 100_000)
        {
            parts.Add($"{IntegerToIndianWords(number / 100_000)} Lakh");
            number %= 100_000;
        }

        if (number >= 1_000)
        {
            parts.Add($"{IntegerToIndianWords(number / 1_000)} Thousand");
            number %= 1_000;
        }

        if (number >= 100)
        {
            parts.Add($"{IntegerToIndianWords(number / 100)} Hundred");
            number %= 100;
        }

        if (number > 0)
        {
            parts.Add(number switch
            {
                1 => "One",
                2 => "Two",
                3 => "Three",
                4 => "Four",
                5 => "Five",
                6 => "Six",
                7 => "Seven",
                8 => "Eight",
                9 => "Nine",
                10 => "Ten",
                11 => "Eleven",
                12 => "Twelve",
                13 => "Thirteen",
                14 => "Fourteen",
                15 => "Fifteen",
                16 => "Sixteen",
                17 => "Seventeen",
                18 => "Eighteen",
                19 => "Nineteen",
                _ => TensWord(number)
            });
        }

        return string.Join(' ', parts);
    }

    private static string TensWord(long number)
    {
        var tens = number / 10;
        var ones = number % 10;

        var tensWord = tens switch
        {
            2 => "Twenty",
            3 => "Thirty",
            4 => "Forty",
            5 => "Fifty",
            6 => "Sixty",
            7 => "Seventy",
            8 => "Eighty",
            9 => "Ninety",
            _ => string.Empty
        };

        var onesWord = ones switch
        {
            1 => "One",
            2 => "Two",
            3 => "Three",
            4 => "Four",
            5 => "Five",
            6 => "Six",
            7 => "Seven",
            8 => "Eight",
            9 => "Nine",
            _ => string.Empty
        };

        return string.IsNullOrEmpty(onesWord) ? tensWord : $"{tensWord} {onesWord}";
    }
}
