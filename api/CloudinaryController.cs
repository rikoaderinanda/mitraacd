using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;
using mitraacd.Services;

namespace mitraacd.api
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Cloudinary")]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryRepository _cloudinaryService;
        private readonly ITaskRepository _TaskService;
        private readonly IAccountRepository _AccountRepo;

        public CloudinaryController(ICloudinaryRepository cloudinaryService,ITaskRepository taskService,IAccountRepository AccountRepo)
        {
            _cloudinaryService = cloudinaryService;
            _TaskService = taskService;
            _AccountRepo = AccountRepo;
        }

        [HttpPost("PhotoBeforeUpload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PhotoBeforeUpload([FromForm] PhotoBeforeUploadDto dto)
        {
            try
            {
                if (dto.ImageFiles  == null || !dto.ImageFiles .Any())
                    return BadRequest(new { message = "Tidak ada file" });
                if (dto.ImageFiles .Count > 3)
                    return BadRequest(new { message = "Maksimal 3 gambar" });
                
                dynamic data = await _TaskService.CheckPhotoSebelumTask(dto.IdTask);
                if(data != null){
                    foreach (var item in data)
                    {
                        var success = await _cloudinaryService.DeleteImageAsync(item.public_id.ToString());
                    }
                    await _TaskService.DeletePhotoSebelumTaskAsync(dto.IdTask);
                }

                var imageResults = new List<ImageUploadResultDto>();

                foreach (var file in dto.ImageFiles )
                {
                    if (file.Length > 5 * 1024 * 1024)
                        return BadRequest("File terlalu besar");
                    if (!file.ContentType.StartsWith("image/"))
                        return BadRequest("Hanya gambar yang diperbolehkan");

                    var (url, publicId) = await _cloudinaryService.UploadImageAsync(file,"UnitBeforeCheck");
                    if (!string.IsNullOrEmpty(url))
                    {
                        var _dt = new ImageUploadResultDto{
                            IdTask = dto.IdTask,
                            Url = url,
                            PublicId = publicId
                        };
                        var res = await _TaskService.SimpanUrlFotoSebelumTask(_dt);
                        if(!res){
                            var del = await _cloudinaryService.DeleteImageAsync(_dt.PublicId);
                            return StatusCode(500, new { message = "Cloudinary upload failed" });    
                        }
                        imageResults.Add(_dt);
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Cloudinary upload failed" });
                    }
                }

                await _TaskService.UpdateStatusTaskAsync(dto.IdTask);
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

        [HttpPost("UploadSelfiPhotoRegistrasi")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSelfiPhotoRegistrasi([FromForm] UploadSelfiePhotoDto dto)
        {
            try
            {
                if (dto.File == null)
                    return BadRequest(new { message = "Tidak ada file" });

                if (dto.File.Length > 5 * 1024 * 1024)
                    return BadRequest("File terlalu besar");

                if (!dto.File.ContentType.StartsWith("image/"))
                    return BadRequest("Hanya gambar yang diperbolehkan");

                var (url, publicId) = await _cloudinaryService.UploadImageAsync(dto.File,"Account/Mitra/PHOTO");
                if (!string.IsNullOrEmpty(url))
                {
                    var dt = new ImageUploadResultDto{
                        IdTask = dto.Id,
                        Url = url,
                        PublicId = publicId
                    };
                    return Ok(new { message = "Upload berhasil", data = dt });
                }
                else
                {
                    return StatusCode(500, new { message = "Cloudinary upload failed" });
                }   
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
            }
        }
        
        [HttpPost("UploadKTP")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadKTP([FromForm] UploadSelfiePhotoDto dto)
        {
            try
            {
                if (dto.File == null)
                    return BadRequest(new { message = "Tidak ada file" });

                if (dto.File.Length > 5 * 1024 * 1024)
                    return BadRequest("File terlalu besar");

                if (!dto.File.ContentType.StartsWith("image/"))
                    return BadRequest("Hanya gambar yang diperbolehkan");

                var (url, publicId) = await _cloudinaryService.UploadImageAsync(dto.File,"Account/Mitra/KTP");
                if (!string.IsNullOrEmpty(url))
                {
                    var dt = new ImageUploadResultDto{
                        IdTask = dto.Id,
                        Url = url,
                        PublicId = publicId
                    };
                    return Ok(new { message = "Upload berhasil", data = dt });
                }
                else
                {
                    return StatusCode(500, new { message = "Cloudinary upload failed" });
                }   
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menyimpan gambar, error:"+ex.Message});
            }
        }

        [HttpPost("deleteFile")]
        public async Task<IActionResult> deleteFile([FromForm] string publicId)
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