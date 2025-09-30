using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextShopV2.Application.DTOs;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IUploadRepository
    {
        Task<UploadResultDto?> UploadAsync(IFormFile file);
        Task<UploadResultDto?> DeleteAsync(string publicId);
        Task<List<UploadResultDto>> UploadMultipleAsync(List<IFormFile> files);
    }
}
