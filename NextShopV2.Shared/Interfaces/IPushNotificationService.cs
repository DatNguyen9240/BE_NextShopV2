using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Shared.Interfaces
{
    public interface IPushNotificationService
    {
        Task RegisterTokenAsync(Guid? userId, string token, string platform, string? deviceId = null);
        Task UnregisterTokenAsync(string token);
        Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null);
        Task SendToTokensAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null);
    }
}