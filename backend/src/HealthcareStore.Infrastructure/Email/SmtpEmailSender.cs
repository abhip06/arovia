using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Infrastructure.Communication;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace HealthcareStore.Infrastructure.Email;

public sealed class SmtpEmailSender(
    IOptions<EmailOptions> options) : IEmailSender
{
    private const string BrandName = "Arovia";

    private readonly EmailOptions _options = options.Value;

    public async Task SendEmailVerificationAsync(
        string email,
        string firstName,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationToken);

        var verificationUrl =
            $"{_options.VerificationBaseUrl}?token={Uri.EscapeDataString(verificationToken)}";

        var body =
            SendEmailVerificationBody(
                firstName,
                verificationUrl);

        using var message = new MailMessage
        {
            From = new MailAddress(
                _options.SmtpUsername,
                _options.SenderName),

            Subject =
                $"Verify your {BrandName} email address",

            Body = body,

            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(email));

        using var smtpClient = new SmtpClient(
            _options.SmtpHost,
            _options.SmtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _options.SmtpUsername,
                _options.SmtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await smtpClient.SendMailAsync(
            message,
            cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        string email,
        string firstName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetToken);

        var resetUrl =
            $"{_options.PasswordResetBaseUrl}?token={Uri.EscapeDataString(resetToken)}";

        var body =
            SendPasswordResetBody(
                firstName,
                resetUrl);

        using var message = new MailMessage
        {
            From = new MailAddress(
                _options.SmtpUsername,
                _options.SenderName),

            Subject =
                $"Reset your {BrandName} password",

            Body = body,

            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(email));

        using var smtpClient = new SmtpClient(
            _options.SmtpHost,
            _options.SmtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _options.SmtpUsername,
                _options.SmtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await smtpClient.SendMailAsync(
            message,
            cancellationToken);
    }

    public async Task SendOrderPaymentConfirmationAsync(
        string email,
        OrderPaymentConfirmationEmail confirmation,
        Stream receiptPdf,
        string receiptFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentNullException.ThrowIfNull(confirmation);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmation.FirstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmation.OrderNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmation.Currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(receiptFileName);
        ArgumentNullException.ThrowIfNull(receiptPdf);

        if (confirmation.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confirmation),
                "The payment amount cannot be negative.");
        }

        if (!receiptPdf.CanRead)
        {
            throw new ArgumentException(
                "The receipt stream must be readable.",
                nameof(receiptPdf));
        }

        var safeFileName = Path.GetFileName(receiptFileName);

        if (!safeFileName.EndsWith(
            ".pdf",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The receipt attachment must be a PDF file.",
                nameof(receiptFileName));
        }

        // Copy into an owned stream so disposing the email attachment
        // does not dispose the caller's stream.
        using var attachmentContent = new MemoryStream();

        if (receiptPdf.CanSeek)
        {
            receiptPdf.Position = 0;
        }

        await receiptPdf.CopyToAsync(
            attachmentContent,
            cancellationToken);

        if (attachmentContent.Length == 0)
        {
            throw new InvalidOperationException(
                "The receipt PDF is empty.");
        }

        attachmentContent.Position = 0;

        using var message = new MailMessage
        {
            From = new MailAddress(
                _options.SmtpUsername,
                _options.SenderName),

            Subject =
                $"Payment confirmed for order {confirmation.OrderNumber} | {BrandName}",

            Body = SendOrderPaymentConfirmationBody(confirmation),

            IsBodyHtml = true,

            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        message.To.Add(new MailAddress(email));

        message.Attachments.Add(
            new Attachment(
                attachmentContent,
                safeFileName,
                MediaTypeNames.Application.Pdf));

        using var smtpClient = new SmtpClient(
            _options.SmtpHost,
            _options.SmtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _options.SmtpUsername,
                _options.SmtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await smtpClient.SendMailAsync(
            message,
            cancellationToken);
    }

    private static string SendEmailVerificationBody(
        string firstName,
        string verificationUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <meta name="color-scheme" content="light dark">
            </head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; line-height: 1.6; margin: 0; padding: 0;">
                <table width="100%" cellpadding="0" cellspacing="0" border="0">
                    <tr>
                        <td align="center" style="padding: 32px 20px;">
                            <table width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width: 600px; text-align: left;">
                                <tr>
                                    <td>
                                        <h1 style="font-size: 26px; font-weight: 700; margin-top: 0; margin-bottom: 24px; color: #43A047;">
                                            {BrandName}
                                        </h1>

                                        <p style="font-size: 16px; margin-bottom: 16px;">
                                            Hello {WebUtility.HtmlEncode(firstName)},
                                        </p>

                                        <p style="font-size: 16px; margin-bottom: 24px;">
                                            Thank you for creating your {BrandName} account. To get started, please verify your email address by clicking the button below.
                                        </p>

                                        <div style="margin: 32px 0;">
                                            <a href="{WebUtility.HtmlEncode(verificationUrl)}" style="display: inline-block; padding: 14px 28px; background-color: #43A047; color: #ffffff; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: 600;">
                                                Verify Email Address
                                            </a>
                                        </div>

                                        <p style="font-size: 16px; margin-bottom: 32px;">
                                            If you did not create this account, you can safely ignore this email.
                                        </p>

                                        <p style="font-size: 15px; margin-bottom: 0;">
                                            Regards,<br />
                                            <strong>{BrandName} Health Team</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    private static string SendPasswordResetBody(
        string firstName,
        string resetUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <meta name="color-scheme" content="light dark">
            </head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; line-height: 1.6; margin: 0; padding: 0;">
                <table width="100%" cellpadding="0" cellspacing="0" border="0">
                    <tr>
                        <td align="center" style="padding: 32px 20px;">
                            <table width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width: 600px; text-align: left;">
                                <tr>
                                    <td>
                                        <h1 style="font-size: 26px; font-weight: 700; margin-top: 0; margin-bottom: 24px; color: #43A047;">
                                            {BrandName} Health
                                        </h1>

                                        <p style="font-size: 16px; margin-bottom: 16px;">
                                            Hello {WebUtility.HtmlEncode(firstName)},
                                        </p>

                                        <p style="font-size: 16px; margin-bottom: 24px;">
                                            We received a request to reset your {BrandName} password. Reset your password by clicking the secure button below.
                                        </p>

                                        <div style="margin: 32px 0;">
                                            <a href="{WebUtility.HtmlEncode(resetUrl)}" style="display: inline-block; padding: 14px 28px; background-color: #43A047; color: #ffffff; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: 600;">
                                                Reset Password
                                            </a>
                                        </div>

                                        <p style="font-size: 14px; margin-bottom: 24px;">
                                            <em>This link is secure and will expire in 1 hour.</em>
                                        </p>

                                        <p style="font-size: 16px; margin-bottom: 32px;">
                                            If you did not request a password reset, you can safely ignore this email. Your account remains secure.
                                        </p>

                                        <p style="font-size: 15px; margin-bottom: 0;">
                                            Regards,<br />
                                            <strong>{BrandName} Health Team</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    private static string SendOrderPaymentConfirmationBody(
    OrderPaymentConfirmationEmail confirmation)
    {
        var firstName =
            WebUtility.HtmlEncode(confirmation.FirstName);

        var orderNumber =
            WebUtility.HtmlEncode(confirmation.OrderNumber);

        var currency =
            WebUtility.HtmlEncode(confirmation.Currency);

        var amount =
            confirmation.Amount.ToString(
                "N2",
                CultureInfo.GetCultureInfo("en-IN"));

        var paymentDate =
            confirmation.PaymentConfirmedAtUtc
                .UtcDateTime
                .ToString(
                    "dd MMM yyyy, HH:mm 'UTC'",
                    CultureInfo.InvariantCulture);

        return $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;font-family:Arial,Helvetica,sans-serif;line-height:1.6;color:#24352b;background-color:#f5f8f6;">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">
                <tr>
                    <td align="center" style="padding:32px 16px;">
                        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:600px;background:#ffffff;border:1px solid #e2eae5;border-radius:12px;">
                            <tr>
                                <td style="padding:32px;">
                                    <h1 style="margin:0 0 8px;color:#16824b;font-size:27px;">
                                        {BrandName}
                                    </h1>

                                    <p style="margin:0 0 28px;color:#687970;font-size:13px;">
                                        Better Health. Made Simple.
                                    </p>

                                    <h2 style="margin:0 0 12px;font-size:22px;">
                                        Payment confirmed!
                                    </h2>

                                    <p style="font-size:15px;">
                                        Hello {firstName},
                                    </p>

                                    <p style="font-size:15px;">
                                        Your payment has been successfully received
                                        for order <strong>#{orderNumber}</strong>.
                                        Your order is confirmed, and we're preparing
                                        it for the next stage.
                                    </p>

                                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:24px 0;background:#f5f8f6;border-radius:10px;">
                                        <tr>
                                            <td style="padding:18px;">
                                                <p style="margin:0 0 5px;color:#687970;font-size:12px;">
                                                    Order number
                                                </p>
                                                <p style="margin:0 0 18px;font-size:15px;font-weight:bold;">
                                                    #{orderNumber}
                                                </p>

                                                <p style="margin:0 0 5px;color:#687970;font-size:12px;">
                                                    Payment received
                                                </p>
                                                <p style="margin:0 0 18px;font-size:24px;font-weight:bold;color:#16824b;">
                                                    {currency} {amount}
                                                </p>

                                                <p style="margin:0 0 5px;color:#687970;font-size:12px;">
                                                    Payment confirmed at
                                                </p>
                                                <p style="margin:0;font-size:14px;">
                                                    {paymentDate}
                                                </p>
                                            </td>
                                        </tr>
                                    </table>

                                    <p style="font-size:14px;">
                                        Your order receipt is attached to this email
                                        as a PDF for your records.
                                    </p>

                                    <p style="margin-top:28px;font-size:14px;">
                                        Thank you for choosing {BrandName}.
                                    </p>

                                    <p style="margin-bottom:0;font-size:14px;">
                                        Regards,<br>
                                        <strong>{BrandName} Health Team</strong>
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }
}