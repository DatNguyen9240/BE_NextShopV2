using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Domain.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NextShopV2.Application.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly ITrackingEventRepository _trackingEventRepo;
        private readonly IShipmentRepository _shipmentRepo;
        private readonly HttpClient _httpClient;

        public TrackingService(
            ITrackingEventRepository trackingEventRepo,
            IShipmentRepository shipmentRepo,
            HttpClient httpClient)
        {
            _trackingEventRepo = trackingEventRepo;
            _shipmentRepo = shipmentRepo;
            _httpClient = httpClient;
        }

        public async Task<List<TrackingEventResponse>> GetTrackingEventsAsync(Guid shipmentId)
        {
            var events = await _trackingEventRepo.GetByShipmentIdAsync(shipmentId);
            return events.Select(MapToResponse).ToList();
        }

        public async Task<bool> AddTrackingEventAsync(Guid shipmentId, string status, string description, string? location = null)
        {
            var trackingEvent = new TrackingEvent
            {
                TrackingEventId = Guid.NewGuid(),
                ShipmentId = shipmentId,
                Status = status,
                Description = description,
                Location = location,
                EventTime = DateTime.UtcNow
            };

            await _trackingEventRepo.AddAsync(trackingEvent);
            await _trackingEventRepo.SaveAsync();

            return true;
        }

        public async Task<bool> SyncTrackingEventsAsync(Guid shipmentId)
        {
            var shipment = await _shipmentRepo.GetByIdAsync(shipmentId);
            if (shipment == null || string.IsNullOrEmpty(shipment.TrackingNumber))
                return false;

            try
            {
                // Call carrier API to get tracking info
                var trackingData = await GetCarrierTrackingDataAsync(shipment.Carrier, shipment.TrackingNumber);

                if (trackingData != null && trackingData.Any())
                {
                    // Convert to TrackingEvent entities
                    var newEvents = trackingData.Select(data => new TrackingEvent
                    {
                        TrackingEventId = Guid.NewGuid(),
                        ShipmentId = shipmentId,
                        Status = data.Status,
                        Description = data.Description,
                        Location = data.Location,
                        EventTime = data.EventTime
                    });

                    await _trackingEventRepo.AddRangeAsync(newEvents);
                    await _trackingEventRepo.SaveAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Failed to sync tracking for shipment {shipmentId}: {ex.Message}");
                return false;
            }
        }

        private async Task<List<CarrierTrackingData>> GetCarrierTrackingDataAsync(string carrier, string trackingNumber)
        {
            // This is a mock implementation. In real world, you would call actual carrier APIs
            // For GHN: https://api.ghn.vn/
            // For Viettel Post, GHTK, etc.

            if (carrier.ToUpper() == "GHN")
            {
                // Mock GHN API call
                return await MockGHNTrackingAsync(trackingNumber);
            }

            return new List<CarrierTrackingData>();
        }

        private async Task<List<CarrierTrackingData>> MockGHNTrackingAsync(string trackingNumber)
        {
            // Simplified tracking for single location business
            // No warehouse transfers, direct from store to customer
            return new List<CarrierTrackingData>
            {
                new CarrierTrackingData
                {
                    Status = "Preparing",
                    Description = "Đơn hàng đang được chuẩn bị",
                    Location = "Cửa hàng",
                    EventTime = DateTime.UtcNow.AddHours(-2)
                },
                new CarrierTrackingData
                {
                    Status = "In transit",
                    Description = "Shipper đang giao hàng đến bạn",
                    Location = "Đang trên đường giao",
                    EventTime = DateTime.UtcNow.AddHours(-1)
                }
            };
        }

        private TrackingEventResponse MapToResponse(TrackingEvent trackingEvent)
        {
            return new TrackingEventResponse
            {
                TrackingEventId = trackingEvent.TrackingEventId,
                Status = trackingEvent.Status,
                Description = trackingEvent.Description,
                Location = trackingEvent.Location,
                EventTime = trackingEvent.EventTime,
                CreatedAt = trackingEvent.CreatedAt
            };
        }
    }

    public class CarrierTrackingData
    {
        public string Status { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Location { get; set; }
        public DateTime EventTime { get; set; }
    }
}