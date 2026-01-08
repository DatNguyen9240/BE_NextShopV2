namespace NextShopV2.Application.DTOs.Request.CreateDto;

public class CreatePaymentRequest
{
    public string OrderCode { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public string BuyerAddress { get; set; } = string.Empty;
    public List<ItemData> Items { get; set; } = new();
    public string? CancelUrl { get; set; }
    public string? ReturnUrl { get; set; }
    public int? ExpiredAt { get; set; }
    public string? Signature { get; set; }
}

public class ItemData
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
}

public class CreateOrderPaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
}