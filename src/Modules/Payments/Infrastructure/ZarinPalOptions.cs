namespace Prescription.Modules.Payments.Infrastructure;

public sealed class ZarinPalOptions
{
    public required string MerchantId { get; init; }
    public string BaseUrl { get; init; } = "https://api.zarinpal.com/pg/v4/payment/";
    public string StartPayUrl { get; init; } = "https://www.zarinpal.com/pg/StartPay/";
}
