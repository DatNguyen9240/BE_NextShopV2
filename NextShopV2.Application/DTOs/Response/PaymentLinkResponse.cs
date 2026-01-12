namespace NextShopV2.Application.DTOs.Response
{
    public class PaymentLinkResponse
    {
        public string? QrCodeUrl { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? ProviderData { get; set; }
        // The provider's order identifier (e.g. PayOS orderCode / paymentId)
        public string? OrderCode { get; set; }
    }
}