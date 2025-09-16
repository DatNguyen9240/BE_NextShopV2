using Microsoft.AspNetCore.Http;

namespace NextShopV2.Api.Models
{
    public class UploadFileRequest
    {
        public IFormFile? File { get; set; }
    }
}