namespace NextShopV2.Application.DTOs.Request.CreateDto
{
    public class AddCartItemDto
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }
}