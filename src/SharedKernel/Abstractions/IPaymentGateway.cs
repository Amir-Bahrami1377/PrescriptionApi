namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Strategy abstraction over the payment gateway. Implemented by Prescription.Modules.Payments
/// (ZarinPalPaymentGateway) so the Orders module never depends on ZarinPal directly.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentRequestResult> RequestPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    Task<PaymentVerificationResult> VerifyPaymentAsync(PaymentVerification verification, CancellationToken cancellationToken = default);
}

public sealed record PaymentRequest(long AmountInRials, string CallbackUrl, string Description, string? PayerMobile);

public sealed record PaymentRequestResult(bool Success, string? Authority, string? PaymentRedirectUrl, string? ErrorMessage);

public sealed record PaymentVerification(string Authority, long AmountInRials);

public sealed record PaymentVerificationResult(bool Success, string? ReferenceId, string? ErrorMessage);
