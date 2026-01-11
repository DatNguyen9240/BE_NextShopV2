namespace NextShopV2.Application.DTOs.Response
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }

    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public object? VariantInfo { get; set; }
    }
}