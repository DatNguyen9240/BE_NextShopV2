using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NextShopV2.Api.Hubs
{
    public class ShipmentTrackingHub : Hub
    {
        // public override Task OnConnectedAsync()
        // {
        //     try
        //     {
        //         // Log connection for tracking
        //         // Console.WriteLine($"Client connected to shipment tracking: {Context.ConnectionId}");
        //         return base.OnConnectedAsync();
        //     }
        //     catch (Exception ex)
        //     {
        //         // Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
        //         throw;
        //     }
        // }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Console.WriteLine($"Client disconnected from shipment tracking: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        // Method for clients to subscribe to specific shipment updates
        public async Task SubscribeToShipment(string shipmentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"shipment-{shipmentId}");
            // Console.WriteLine($"Client {Context.ConnectionId} subscribed to shipment {shipmentId}");
        }

        // Method for clients to unsubscribe from shipment updates
        public async Task UnsubscribeFromShipment(string shipmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"shipment-{shipmentId}");
            // Console.WriteLine($"Client {Context.ConnectionId} unsubscribed from shipment {shipmentId}");
        }

        // Method for shippers to update their location for a shipment
        public async Task UpdateLocation(string shipmentId, double latitude, double longitude)
        {
            // Broadcast location update to all clients subscribed to this shipment
            await Clients.Group($"shipment-{shipmentId}").SendAsync("LocationUpdate", shipmentId, latitude, longitude);
            // Console.WriteLine($"Location update for shipment {shipmentId}: {latitude}, {longitude}");
        }
    }
}