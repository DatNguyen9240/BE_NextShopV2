using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Repositories
{
    public class UploadRepository : IUploadRepository
    {
        private readonly Cloudinary _cloudinary;
        public UploadRepository(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<UploadResultDto> UploadAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "banners"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new UploadResultDto
                {
                    Url = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId
                };
            }
            // Lỗi: trả về null, controller sẽ xử lý lỗi
            return null;
        }

        public async Task<List<UploadResultDto>> UploadMultipleAsync(List<IFormFile> files)
        {
            var results = new List<UploadResultDto>();
            foreach (var file in files)
            {
                var result = await UploadAsync(file);
                results.Add(result);
            }
            return results;
        }

        public async Task<UploadResultDto> DeleteAsync(string publicId)
        {
            // Cloudinary không trả về Url khi xóa, nên chỉ trả về PublicId
            var deletionParams = new DeletionParams(publicId);
            var result = await Task.Run(() => _cloudinary.Destroy(deletionParams));
            if (result.Result == "ok")
            {
                return new UploadResultDto
                {
                    Url = null,
                    PublicId = publicId
                };
            }
            return null;
        }
    }
}
