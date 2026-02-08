using System;

namespace NextShopV2.Domain.Entities
{
    public class TaxSetting
    {
        public Guid SettingId { get; set; }
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
