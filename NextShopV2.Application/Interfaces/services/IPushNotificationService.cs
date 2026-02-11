using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Request.CreateDto;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IPushNotificationService
    {
        Task SaveTokenAsync(FirebaseFcmTokenModel model, string? userId);
        Task<string> SendNotificationAsync(string token, FirebaseNotificationRequest request);
        Task<FirebaseNotificationSendResult> SendToAllAsync(FirebaseNotificationRequest request);
        Task<IEnumerable<FirebaseFcmTokenDto>> GetTokensAsync();
        Task<IEnumerable<FirebaseNotificationHistoryDto>> GetHistoryAsync(int count = 50);
        Task TestFirebaseAsync();
        Task ClearTokensAsync();

        // Send notification to tokens owned by a specific user
        Task<FirebaseNotificationSendResult> SendToUserAsync(Guid userId, FirebaseNotificationRequest request);
    }
}