using Microsoft.AspNetCore.Http;

namespace NextShopV2.Application.DTOs.Request
{
    public class UploadFileRequest
    {
        public IFormFile? File { get; set; }
    }
}