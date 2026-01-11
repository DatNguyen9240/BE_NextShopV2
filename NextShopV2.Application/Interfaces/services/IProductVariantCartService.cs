using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;

namespace NextShopV2.Application.Interfaces.Services
{
    // Interface for getting variant info for cart
    public interface IProductVariantCartService
    {
        Task<VariantInfo?> GetVariantInfoAsync(Guid variantId);
    }
}