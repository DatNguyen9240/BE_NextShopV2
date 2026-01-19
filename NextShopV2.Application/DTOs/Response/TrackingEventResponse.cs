using System;

namespace NextShopV2.Application.DTOs.Response
{
    public class TrackingEventResponse
    {
        public Guid TrackingEventId { get; set; }
        public string Status { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Location { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}