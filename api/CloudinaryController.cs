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
        private readonly ITaskRepository _TaskService;

        public CloudinaryController(ICloudinaryRepository cloudinaryService,ITaskRepository taskService)
        {
            _cloudinaryService = cloudinaryService;
            _TaskService = taskService;
        }

        [HttpPost("PhotoBeforeUpload")]
        public async Task<IActionResult> PhotoBeforeUpload(
            [FromForm] List<IFormFile> imageFiles,
            [FromForm] string IdTask)
        {
            try
            {
                if (imageFiles == null || !imageFiles.Any())
                    return BadRequest(new { message = "Tidak ada file" });
                if (imageFiles.Count > 3)
                    return BadRequest(new { message = "Maksimal 3 gambar" });
                
                dynamic data = await _TaskService.CheckPhotoSebelumTask(IdTask);
                if(data != null){
                    foreach (var item in data)
                    {
                        var success = await _cloudinaryService.DeleteImageAsync(item.public_id.ToString());
                    }
                    await _TaskService.DeletePhotoSebelumTaskAsync(IdTask);
                }

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
                        var dto = new ImageUploadResultDto{
                            IdTask = IdTask,
                            Url = url,
                            PublicId = publicId
                        };
                        var res = await _TaskService.SimpanUrlFotoSebelumTask(dto);
                        if(!res){
                            var del = await _cloudinaryService.DeleteImageAsync(dto.PublicId);
                            return StatusCode(500, new { message = "Cloudinary upload failed" });    
                        }
                        imageResults.Add(dto);
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Cloudinary upload failed" });
                    }
                }

                await _TaskService.UpdateStatusTaskAsync(IdTask);
                return Ok(new { message = "Upload berhasil", data = imageResults });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
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