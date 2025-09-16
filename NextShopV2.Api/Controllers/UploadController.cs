using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NextShopV2.Api.Models;
namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;

        public UploadController(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "banners"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(new { 
                    url = uploadResult.SecureUrl.ToString(),
                    publicId = uploadResult.PublicId
                });

            return StatusCode(500, "Upload failed.");
        }

        [HttpDelete]
        public IActionResult Delete([FromQuery] string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = _cloudinary.Destroy(deletionParams);

            if (result.Result == "ok")
                return Ok(new { success = true });
            return BadRequest(new { success = false, message = result.Error?.Message });
        }

        [HttpPost("multi")]
        public async Task<IActionResult> UploadMultiple([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var results = new List<object>();

            foreach (var file in files)
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
                    results.Add(new
                    {
                        url = uploadResult.SecureUrl.ToString(),
                        publicId = uploadResult.PublicId
                    });
                }
                else
                {
                    results.Add(new
                    {
                        error = uploadResult.Error?.Message ?? "Upload failed.",
                        file = file.FileName
                    });
                }
            }

            return Ok(results);
        }
    }
}