using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Request.CreateDto;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IFirebaseNotificationService
    {
        Task<string> SendNotificationAsync(string token, FirebaseNotificationRequest notification);
        Task<FirebaseNotificationSendResult> SendToMultipleAsync(List<string> tokens, FirebaseNotificationRequest notification);
        Task<string> SendToTopicAsync(string topic, FirebaseNotificationRequest notification);
    }
}