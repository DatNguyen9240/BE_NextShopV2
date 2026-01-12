using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<bool> HandlePayOSWebhookAsync(string body, string? signature, string checksumKey);
    PaymentStatusResponse? GetPaymentStatus(string orderCode);
    PaymentStatusResponse? GetPaymentStatusByOrderId(Guid orderId);
    Task<PaymentLinkResponse> CreatePaymentForOrderAsync(Guid orderId);

    // Mark a payment (including COD) as paid/collected. Returns true if successful.
    Task<bool> MarkPaymentAsPaidAsync(Guid paymentId, string? collectedBy);
}