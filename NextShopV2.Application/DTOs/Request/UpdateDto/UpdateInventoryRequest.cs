namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateInventoryRequest
    {
        public Guid VariantId { get; set; }
        public int ChangeQty { get; set; }
        public string Reason { get; set; } = null!;
    }
}