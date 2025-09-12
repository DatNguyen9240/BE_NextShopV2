using System;

namespace NextShopV2.Domain.Entities.Marketing
{
    public class Advertisement
    {
        public Guid AdvertisementId { get; set; }


        public string Title { get; set; } = string.Empty;


        public string? Description { get; set; }


        public string MediaUrl { get; set; } = string.Empty;


        public string TargetUrl { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;


        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

   
        public bool IsActive { get; set; } = true;
    }
}
