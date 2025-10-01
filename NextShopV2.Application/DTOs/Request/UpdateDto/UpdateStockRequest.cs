using System.ComponentModel.DataAnnotations;

namespace NextShopV2.Application.DTOs.Request
{
    public class UpdateStockRequest
    {
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}