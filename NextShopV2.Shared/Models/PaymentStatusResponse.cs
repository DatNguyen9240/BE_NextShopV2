namespace NextShopV2.Shared.Models;

public class PaymentStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string? ProviderData { get; set; }
}