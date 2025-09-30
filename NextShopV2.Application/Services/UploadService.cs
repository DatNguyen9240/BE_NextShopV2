using Microsoft.AspNetCore.Http;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Services
{
    public class UploadService : IUploadService
    {
        private readonly IUploadRepository _uploadRepository;
        public UploadService(IUploadRepository uploadRepository)
        {
            _uploadRepository = uploadRepository;
        }
        public Task<UploadResultDto?> UploadAsync(IFormFile file)
        {
            return _uploadRepository.UploadAsync(file);
        }
        public Task<List<UploadResultDto>> UploadMultipleAsync(List<IFormFile> files)
        {
            return _uploadRepository.UploadMultipleAsync(files);
        }
        public Task<UploadResultDto?> DeleteAsync(string publicId)
        {
            return _uploadRepository.DeleteAsync(publicId);
        }
    }
}
