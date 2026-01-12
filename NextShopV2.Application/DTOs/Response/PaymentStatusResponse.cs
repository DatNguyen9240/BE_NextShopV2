namespace NextShopV2.Application.DTOs.Response;

public class PaymentStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string? ProviderData { get; set; }
}