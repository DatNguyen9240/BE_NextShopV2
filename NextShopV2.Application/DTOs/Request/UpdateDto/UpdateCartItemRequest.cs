namespace NextShopV2.Application.DTOs.Request.UpdateDto
{
    public class UpdateCartItemDto
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}