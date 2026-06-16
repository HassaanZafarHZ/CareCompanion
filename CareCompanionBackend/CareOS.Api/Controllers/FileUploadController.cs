using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        // POST: api/FileUpload/image?folder=profiles
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromQuery] string folder = "general")
        {
            var result = await _fileUploadService.UploadImageAsync(file, folder);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/FileUpload/base64
        [HttpPost("base64")]
        public async Task<IActionResult> UploadBase64Image([FromBody] Base64ImageDto request)
        {
            var result = await _fileUploadService.UploadBase64ImageAsync(
                request.Base64String,
                request.Folder,
                request.FileName
            );

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // DELETE: api/FileUpload?filePath=/Uploads/profiles/abc.jpg
        [HttpDelete]
        public async Task<IActionResult> DeleteImage([FromQuery] string filePath)
        {
            var result = await _fileUploadService.DeleteImageAsync(filePath);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

    public class Base64ImageDto
    {
        public string Base64String { get; set; } = string.Empty;
        public string Folder { get; set; } = "general";
        public string FileName { get; set; } = "image";
    }
}