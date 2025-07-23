using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryRepository _cloudinaryService;

        public CloudinaryController(ICloudinaryRepository cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("PhotoBeforeUpload")]
        public async Task<IActionResult> PhotoBeforeUpload(List<IFormFile> imageFiles)
        {
            try
            {
                if (imageFiles == null || !imageFiles.Any())
                    return BadRequest(new { message = "Tidak ada file" });
                if (imageFiles.Count > 3)
                    return BadRequest(new { message = "Maksimal 3 gambar" });

                var imageResults = new List<ImageUploadResultDto>();

                foreach (var file in imageFiles)
                {
                    if (file.Length > 5 * 1024 * 1024)
                        return BadRequest("File terlalu besar");
                    if (!file.ContentType.StartsWith("image/"))
                        return BadRequest("Hanya gambar yang diperbolehkan");

                    var (url, publicId) = await _cloudinaryService.UploadImageAsync(file,"UnitBeforeCheck");
                    if (!string.IsNullOrEmpty(url))
                    {
                        imageResults.Add(new ImageUploadResultDto
                        {
                            Url = url,
                            PublicId = publicId
                        });
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Cloudinary upload failed" });
                    }
                }

                return Ok(new { message = "Upload berhasil", data = imageResults });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar" });
            }
        }

        [HttpPost("deletePhotoBefore")]
        public async Task<IActionResult> deletePhotoBefore([FromForm] string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return BadRequest(new { message = "public_id kosong" });

            var success = await _cloudinaryService.DeleteImageAsync(publicId);

            if (!success)
                return StatusCode(500, new { message = "Gagal menghapus gambar" });

            return Ok(new { message = "Gambar berhasil dihapus" });
        }
    }
}