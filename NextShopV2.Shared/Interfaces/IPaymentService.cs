using NextShopV2.Shared.Models;

namespace NextShopV2.Shared.Interfaces;

public interface IPaymentService
{
    Task<bool> HandlePayOSWebhookAsync(string body, string? signature, string checksumKey);
    PaymentStatusResponse? GetPaymentStatus(string orderCode);
    PaymentStatusResponse? GetPaymentStatusByOrderId(Guid orderId);
    Task<NextShopV2.Shared.Models.PaymentLinkResponse> CreatePaymentForOrderAsync(Guid orderId);

    // Mark a payment (including COD) as paid/collected. Returns true if successful.
    Task<bool> MarkPaymentAsPaidAsync(Guid paymentId, string? collectedBy);
}