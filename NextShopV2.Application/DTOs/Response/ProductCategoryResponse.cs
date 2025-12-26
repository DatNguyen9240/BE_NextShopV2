using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class ProductCategoryResponse
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}