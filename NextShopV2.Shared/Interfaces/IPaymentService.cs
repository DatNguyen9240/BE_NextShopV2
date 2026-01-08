using NextShopV2.Shared.Models;

namespace NextShopV2.Shared.Interfaces;

public interface IPaymentService
{
    Task<bool> HandlePayOSWebhookAsync(string body, string? signature, string checksumKey);
    PaymentStatusResponse? GetPaymentStatus(string orderCode);
    PaymentStatusResponse? GetPaymentStatusByOrderId(Guid orderId);
    Task<NextShopV2.Shared.Models.PaymentLinkResponse> CreatePaymentForOrderAsync(Guid orderId);
}