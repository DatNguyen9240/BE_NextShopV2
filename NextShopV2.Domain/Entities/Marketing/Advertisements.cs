using System;
using System.Collections.Generic;

namespace NextShopV2.Domain.Entities.Marketing
{
    public class Advertisement
    {
        public Guid Id { get; set; }
        public string PublicId { get; set; } = ""; // Thêm trường PublicId để lưu mã public (ví dụ Cloudinary)
        public string Title { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Type { get; set; } = "";      // Ví dụ: "home", "product"
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
