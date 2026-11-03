namespace HealthcareStore.Application.Abstractions.Communication;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        string email,
        string firstName,
        string verificationToken,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string email,
        string firstName,
        string resetToken,
        CancellationToken cancellationToken = default);

    Task SendOrderPaymentConfirmationAsync(
        string email,
        OrderPaymentConfirmationEmail confirmation,
        Stream receiptPdf,
        string receiptFileName,
        CancellationToken cancellationToken = default);
}