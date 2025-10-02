namespace NextShopV2.Application.DTOs.Response
{
    public class InventoryTransactionResponse
    {
        public Guid TransactionId { get; set; }
        public Guid VariantId { get; set; }
        public string VariantSKU { get; set; } = null!;
        public int ChangeQty { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
    }
}