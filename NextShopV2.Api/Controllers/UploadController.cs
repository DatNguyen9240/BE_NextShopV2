using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Shared.Extensions;
namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
        {
            if (request.File is null || request.File.Length == 0)
                return Ok(new ApiResponse { Success = false, Message = "No file uploaded." });

            var result = await _uploadService.UploadAsync(request.File);
            if (result is not null)
            {
                return Ok(new ApiResponse { 
                    Success = true, 
                    Message = "Tải thành công", 
                    Data = new { 
                        url = result.Url,        
                        publicId = result.PublicId
                    } 
                });
            }
            return BadRequest(new ApiResponse { Success = false, Message = "Upload failed." });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string publicId)
        {
            var result = await _uploadService.DeleteAsync(publicId);
            if (result is not null)
            {
                return Ok(new ApiResponse { Success = true, Message = "Xóa thành công", Data = new { publicId = result.PublicId } });
            }
            return BadRequest(new ApiResponse { Success = false, Message = "Delete failed." });
        }

        [HttpPost("multi")]
        public async Task<IActionResult> UploadMultiple([FromForm] List<IFormFile> files)
        {
            if (files.IsNullOrEmpty())
                return Ok(new ApiResponse { Success = false, Message = "No files uploaded." });

            var results = await _uploadService.UploadMultipleAsync(files);
            return Ok(new ApiResponse { Success = true, Data = results });
        }
    }
}